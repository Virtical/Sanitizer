# План обработки десанитизации (детокенизации) в режиме streaming

## Проблема

В обычном (non-streaming) режиме весь ответ LLM приходит одной строкой,
и `DesanitizerService.Desanitize(raw, context)` просто заменяет все токены
обратно на исходные значения.

В режиме streaming ответ приходит по частям (`update.Text`). Токен (например,
`__TOKEN_PHONE_1__`) может быть разорван между двумя соседними чанками:

```
chunk[i]   = "...ваш номер __TOK"
chunk[i+1] = "EN_PHONE_1__ уже сохранён..."
```

Если применять `Desanitize` к каждому чанку независимо, токены на границах
не будут заменены и утекут в клиента в «сыром» виде. Также нельзя просто
накапливать весь ответ и применять `Desanitize` в конце — это сводит на нет
сам смысл стриминга.

## Решение: потоковый детокенизатор с буфером границы

Идея: поддерживать небольшой «хвостовой» буфер, в котором мог бы поместиться
любой токен целиком. Из буфера в клиента отдаём только ту часть, в которой
гарантированно нет начала незавершённого токена.

### Алгоритм

1. Все токены имеют известный формат (например, `__TOKEN_<TYPE>_<N>__`).
   Из формата выводим:
   - префикс начала токена (например, `__TOKEN_`);
   - максимальную возможную длину токена `MaxTokenLength` (можно вычислить
     по `SanitizationContext`, взяв max длины ключей + запас).

2. Заводим строковый буфер `buffer` (StringBuilder).

3. На каждый `update.Text`:
   1. Добавляем текст в `buffer`.
   2. Применяем `Desanitize` к содержимому `buffer` —
      все *полные* токены заменяются на исходные значения.
   3. Находим в буфере позицию `safeEnd` — индекс, начиная с которого может
      располагаться *незавершённый* токен:
      - ищем последнее вхождение префикса токена (`__TOKEN_`);
      - если найдено и от этой позиции до конца буфера меньше
        `MaxTokenLength` символов — `safeEnd` = эта позиция;
      - иначе `safeEnd` = длина буфера.
   4. Отправляем клиенту подстроку `buffer[0..safeEnd]`,
      удаляем эту часть из буфера.
   5. Flush.

4. После завершения стрима:
   - применяем `Desanitize` к остатку буфера (на случай если последний токен
     не закрылся — отдаём как есть);
   - отправляем остаток клиенту;
   - сохраняем полный собранный ответ в историю чата.

### Псевдокод

```csharp
var buffer = new StringBuilder();
var full = new StringBuilder();
const string tokenPrefix = "__TOKEN_";
int maxTokenLen = context.MaxTokenLength; // вычислить заранее

await foreach (var update in chatClient.GetStreamingResponseAsync(...))
{
    if (string.IsNullOrEmpty(update.Text)) continue;

    buffer.Append(update.Text);

    // Заменяем все полные токены
    var replaced = desanitizer.Desanitize(buffer.ToString(), context);
    buffer.Clear();
    buffer.Append(replaced);

    // Ищем потенциально незавершённый токен в конце
    int safeEnd = buffer.Length;
    int idx = LastIndexOf(buffer, tokenPrefix);
    if (idx >= 0 && buffer.Length - idx < maxTokenLen)
        safeEnd = idx;

    if (safeEnd > 0)
    {
        var toSend = buffer.ToString(0, safeEnd);
        await Response.WriteAsync(toSend);
        await Response.Body.FlushAsync();
        full.Append(toSend);
        buffer.Remove(0, safeEnd);
    }
}

// Финал: отдать остаток
var tail = desanitizer.Desanitize(buffer.ToString(), context);
if (tail.Length > 0)
{
    await Response.WriteAsync(tail);
    await Response.Body.FlushAsync();
    full.Append(tail);
}

await chatHistoryService.AddMessageAsync(id, MessageRequest.CreateAnswer(full.ToString()));
```

## Изменения в коде

### 1. `DesanitizerService` (или новый `StreamingDesanitizer`)

Добавить стримовый враппер, инкапсулирующий логику буфера:

```csharp
public sealed class StreamingDesanitizer
{
    private readonly DesanitizerService _inner;
    private readonly SanitizationContext _context;
    private readonly StringBuilder _buffer = new();
    private readonly int _maxTokenLen;
    private const string TokenPrefix = "__TOKEN_";

    public StreamingDesanitizer(DesanitizerService inner, SanitizationContext ctx)
    {
        _inner = inner;
        _context = ctx;
        _maxTokenLen = ctx.MaxTokenLength; // или вычислить по ключам
    }

    /// <summary>Принимает очередной чанк, возвращает безопасную часть для отправки клиенту.</summary>
    public string Push(string chunk)
    {
        _buffer.Append(chunk);
        var replaced = _inner.Desanitize(_buffer.ToString(), _context);
        _buffer.Clear();
        _buffer.Append(replaced);

        int safeEnd = _buffer.Length;
        int idx = LastIndexOf(_buffer, TokenPrefix);
        if (idx >= 0 && _buffer.Length - idx < _maxTokenLen)
            safeEnd = idx;

        if (safeEnd == 0) return string.Empty;

        var result = _buffer.ToString(0, safeEnd);
        _buffer.Remove(0, safeEnd);
        return result;
    }

    /// <summary>Возвращает оставшийся хвост (вызывать в конце стрима).</summary>
    public string Flush()
    {
        var tail = _inner.Desanitize(_buffer.ToString(), _context);
        _buffer.Clear();
        return tail;
    }
}
```

### 2. `ChatController.Send`

Заменить текущий цикл:

```csharp
var streaming = new StreamingDesanitizer(desanitizer, sanitization.Context);
var full = new StringBuilder();

await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options))
{
    if (string.IsNullOrEmpty(update.Text)) continue;

    var safe = streaming.Push(update.Text);
    if (safe.Length > 0)
    {
        await Response.WriteAsync(safe);
        await Response.Body.FlushAsync();
        full.Append(safe);
    }
}

var tail = streaming.Flush();
if (tail.Length > 0)
{
    await Response.WriteAsync(tail);
    await Response.Body.FlushAsync();
    full.Append(tail);
}

await chatHistoryService.AddMessageAsync(id, MessageRequest.CreateAnswer(full.ToString()));
```

Десанитизированный ответ сохраняется в историю (а не сырой с токенами).

### 3. Гарантии формата токена

Чтобы алгоритм работал надёжно:

- Формат токена должен быть фиксированный, не пересекаться с обычным текстом
  (длинный префикс, например `__TOKEN_`).
- В `SanitizationContext` хранить `MaxTokenLength` (вычислять при создании
  контекста — максимальная длина среди ключей замен + небольшой запас).
- Префикс токена не должен встречаться внутри значений других токенов.

## Граничные случаи

1. **Токен на самой границе чанка** — обрабатывается буфером.
2. **Несколько токенов подряд** — `Desanitize` заменит все полные за один проход.
3. **Чанк длиннее `MaxTokenLength`, не содержит начала токена** — `safeEnd`
   равен длине буфера, всё уходит клиенту сразу.
4. **Поток оборвался посередине токена** — `Flush` пытается заменить остаток;
   если не получилось — сырая последовательность уходит в клиент как fallback
   (можно дополнительно логировать как ошибку).
5. **LLM «галлюцинирует» несуществующий токен** — `Desanitize` оставит его
   как есть; это поведение задаётся в `DesanitizerService` (по умолчанию
   неизвестные токены сохраняются дословно).

## Тесты

Создать unit-тесты для `StreamingDesanitizer`:

- токен полностью в одном чанке;
- токен разорван между двумя чанками;
- токен разорван между N чанками (по 1 символу);
- два токена подряд;
- пустые чанки;
- незакрытый токен в конце стрима;
- текст без токенов (стрим проходит без задержек на буфере).
