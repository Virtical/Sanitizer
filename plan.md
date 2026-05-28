# План обработки десанитизации (детокенизации) в режиме streaming

## Формат токена

Токены имеют формат `[<TYPE>_<N>]` (см. `TokenStore.GetOrCreate`),
например `[PHONE_1]`, `[EMAIL_2]`. Максимальная длина токена — **15 символов**
(включая обе квадратные скобки).

## Проблема

В обычном (non-streaming) режиме весь ответ LLM приходит одной строкой,
и `DesanitizerService.Desanitize(raw, context)` просто заменяет все токены
обратно на исходные значения.

В режиме streaming ответ приходит по частям (`update.Text`). Токен может быть
разорван между двумя соседними чанками:

```
chunk[i]   = "...ваш номер [PHO"
chunk[i+1] = "NE_1] уже сохранён..."
```

Если применять `Desanitize` к каждому чанку независимо, токены на границах
не будут заменены и утекут в клиента в «сыром» виде. Также нельзя просто
накапливать весь ответ и применять `Desanitize` в конце — это сводит на нет
сам смысл стриминга.

## Решение: потоковый детокенизатор с буфером по скобкам

Идея: отдаём клиенту всё до последней открывающей скобки `[`, у которой ещё
не встретилась закрывающая `]`. Незакрытый «хвост» (от `[` до конца буфера)
держим до тех пор, пока:

- не придёт закрывающая `]` (токен сформирован — выполняем `Desanitize` и
  отдаём результат);
- либо длина незакрытого фрагмента не превысит **15 символов** — значит, это
  точно не наш токен, отдаём содержимое клиенту как есть.

### Константа

```csharp
private const int MaxTokenLength = 15; // [TYPE_N]
```

### Алгоритм на каждый чанк

1. Добавляем `chunk` к буферу `_buffer`.
2. Ищем последнюю открывающую `[` в буфере.
3. **Случай A.** `[` не найдена — весь буфер «безопасен»:
   отдаём его клиенту, очищаем.
4. **Случай B.** `[` найдена в позиции `openIdx`:
   - если *после* `openIdx` в буфере встречается `]` — токен полностью
     внутри буфера; применяем `Desanitize` ко всему буферу и отдаём результат
     клиенту, очищаем буфер;
   - если `]` нет, но `_buffer.Length - openIdx > MaxTokenLength` — открывающая
     скобка точно не относится к токену; отдаём весь буфер как есть, очищаем;
   - иначе (есть `[`, нет `]`, длина «хвоста» ≤ 15) — отдаём клиенту
     `_buffer[0..openIdx]`, остаток оставляем в буфере, ждём следующий чанк.

> ⚠️ Важно: проверять нужно именно *последнюю* открывающую `[`. Если в чанке
> пришло `"... [PHONE_1] и ещё [EM"`, до первой `]` всё уже десанитизируется,
> а в буфере остаётся только `"[EM"`.
>
> На практике проще делать так: пока в буфере есть открывающая `[`, у которой
> есть парная `]`, прогоняем `Desanitize` и продвигаемся вперёд. Затем смотрим
> на оставшуюся незакрытую `[`.

### Финализация стрима

После завершения `await foreach`:

- если в буфере остался незакрытый хвост — отдаём его клиенту как есть
  (это либо обычный текст с `[`, либо «битый» токен — в любом случае не
  тянем дальше);
- сохраняем полный собранный (уже десанитизированный) ответ в историю чата.

### Псевдокод

```csharp
const int MaxTokenLength = 15;

var buffer = new StringBuilder();
var full   = new StringBuilder();

async Task FlushSafe()
{
    while (true)
    {
        int openIdx = LastIndexOf(buffer, '[');

        if (openIdx < 0)
        {
            // нет открывающей скобки — всё безопасно
            await Emit(buffer.ToString());
            buffer.Clear();
            return;
        }

        int closeIdx = IndexOf(buffer, ']', openIdx + 1);

        if (closeIdx >= 0)
        {
            // токен полностью в буфере — десанитизируем и отдаём весь буфер
            var replaced = desanitizer.Desanitize(buffer.ToString(), context);
            await Emit(replaced);
            buffer.Clear();
            return;
        }

        if (buffer.Length - openIdx > MaxTokenLength)
        {
            // это не наш токен — отдаём всё
            await Emit(buffer.ToString());
            buffer.Clear();
            return;
        }

        // ждём закрывающую скобку — отдаём всё до openIdx, остальное держим
        if (openIdx > 0)
        {
            await Emit(buffer.ToString(0, openIdx));
            buffer.Remove(0, openIdx);
        }
        return;
    }
}

async Task Emit(string s)
{
    if (s.Length == 0) return;
    await Response.WriteAsync(s);
    await Response.Body.FlushAsync();
    full.Append(s);
}

await foreach (var update in chatClient.GetStreamingResponseAsync(messages, options))
{
    if (string.IsNullOrEmpty(update.Text)) continue;
    buffer.Append(update.Text);
    await FlushSafe();
}

// финал: отдать остаток как есть
if (buffer.Length > 0)
{
    await Emit(buffer.ToString());
    buffer.Clear();
}

await chatHistoryService.AddMessageAsync(id, MessageRequest.CreateAnswer(full.ToString()));
```

## Изменения в коде

### 1. Новый класс `StreamingDesanitizer`

Файл: `Sanitizer.Api/Services/StreamingDesanitizer.cs`.

```csharp
public sealed class StreamingDesanitizer
{
    private const int MaxTokenLength = 15; // [TYPE_N]

    private readonly DesanitizerService _inner;
    private readonly SanitizationContext _context;
    private readonly StringBuilder _buffer = new();

    public StreamingDesanitizer(DesanitizerService inner, SanitizationContext context)
    {
        _inner = inner;
        _context = context;
    }

    /// <summary>Принимает чанк, возвращает безопасную часть для отправки клиенту.</summary>
    public string Push(string chunk)
    {
        _buffer.Append(chunk);

        int openIdx = LastIndexOf(_buffer, '[');

        // 1. открывающей скобки нет — весь буфер безопасен
        if (openIdx < 0)
            return Drain();

        int closeIdx = IndexOf(_buffer, ']', openIdx + 1);

        // 2. парная закрывающая есть — токен полностью внутри буфера
        if (closeIdx >= 0)
        {
            var replaced = _inner.Desanitize(_buffer.ToString(), _context);
            _buffer.Clear();
            return replaced;
        }

        // 3. незакрытый хвост слишком длинный — это не токен
        if (_buffer.Length - openIdx > MaxTokenLength)
            return Drain();

        // 4. ждём закрывающую: отдаём всё до openIdx
        if (openIdx == 0) return string.Empty;
        var safe = _buffer.ToString(0, openIdx);
        _buffer.Remove(0, openIdx);
        return safe;
    }

    /// <summary>Возвращает оставшийся хвост (вызывать в конце стрима).</summary>
    public string Flush()
    {
        // На всякий случай прогоняем десанитизацию — вдруг там полный токен.
        var tail = _inner.Desanitize(_buffer.ToString(), _context);
        _buffer.Clear();
        return tail;
    }

    private string Drain()
    {
        var s = _buffer.ToString();
        _buffer.Clear();
        return s;
    }

    private static int LastIndexOf(StringBuilder sb, char c)
    {
        for (int i = sb.Length - 1; i >= 0; i--)
            if (sb[i] == c) return i;
        return -1;
    }

    private static int IndexOf(StringBuilder sb, char c, int start)
    {
        for (int i = start; i < sb.Length; i++)
            if (sb[i] == c) return i;
        return -1;
    }
}
```

### 2. `ChatController.Send`

Заменить текущий цикл стриминга:

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

В историю чата уходит уже десанитизированный текст.

## Граничные случаи

1. **`[` на самой границе чанка** — попадает в буфер, ждём `]`.
2. **`]` нет, пришло > 15 символов после `[`** — это обычный текст с `[`,
   отдаём как есть.
3. **Несколько токенов подряд `[A_1][B_2]`** — после прихода первой `]`
   десанитизируем весь буфер за один проход (`Desanitize` работает по всем
   ключам контекста).
4. **`[` встречается в обычном тексте (markdown-ссылка, JSON и т.п.)** —
   если после неё нет `]` в пределах 15 символов, текст отдаётся клиенту;
   если внутри 15 символов есть `]`, но это не токен — `Desanitize` оставит
   фрагмент как есть (неизвестные токены не меняются).
5. **Поток оборвался посередине токена** — `Flush` пробует десанитизацию,
   если не получилось — содержимое уходит клиенту как есть.
6. **LLM «галлюцинирует» несуществующий токен `[FOO_99]`** — пройдёт через
   `Desanitize` без изменений.

## Тесты

Юнит-тесты для `StreamingDesanitizer`:

- токен полностью в одном чанке;
- токен разорван между двумя чанками (`"... [PHO"` + `"NE_1] ..."`);
- токен разорван посимвольно (N чанков по 1 символу);
- два токена подряд (`"[A_1][B_2]"`);
- одиночная `[` в обычном тексте (markdown `[ссылка](...)`);
- `[` без `]` дольше 15 символов — должен «прорваться» наружу;
- пустые чанки;
- незакрытый токен в самом конце стрима;
- текст без скобок (буфер не задерживает чанки).
