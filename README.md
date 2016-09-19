# Shaman.WinForms.Dialogs
Async/await-based versions of dialog boxes (avoids reentrancy/focus problems)

```csharp
using Shaman.WinForms;

await Dialogs.ShowMessageAsync(owner, text, caption, MessageBoxButtons.OK);

```

