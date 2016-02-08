using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Shaman.WinForms;

namespace Shaman.WinForms
{
    public static class Dialogs
    {

        public static async Task<DialogResult> ShowMessageAsync(
            IWin32Window owner,
            string text,
            string caption = null,
            MessageBoxButtons buttons = MessageBoxButtons.OK,
            MessageBoxIcon icon = MessageBoxIcon.None,
            MessageBoxDefaultButton defaultButton = MessageBoxDefaultButton.Button1
            )
        {
            var form = owner as Form;
            if (form != null)
            {
                if (IsClosedOrNullOrHidden(form)) owner = null;
                else if (caption == null) caption = form.Text;
            }



            return await Task.Run(() =>
            {
                using (new CrossThreadChecksWaiver())
                {
                    var result = MessageBox.Show(owner, text, caption, buttons, icon, defaultButton);
                    return result;
                }
            });
        }


        public static Task<T> RunAsyncSTA<T>(Func<T> action)
        {
            var tcs = new TaskCompletionSource<T>();

            var t = new Thread(() =>
            {
                T result = default(T);
                try
                {
                    result = action();
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                    return;
                }
                tcs.TrySetResult(result);
            });
            t.IsBackground = true;
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            return tcs.Task;
        }

        internal static bool IsClosedOrNullOrHidden(Form f)
        {
            return f == null || f.Disposing || f.IsDisposed || !f.Visible;
        }

        public static Task<string> BrowseForFolderAsync(
                IWin32Window owner,
                string description,
                string selectedPath = null,
                bool showNewFolderButton = false
            )
        {
            if (IsClosedOrNullOrHidden(owner as Form)) owner = null;

            return RunAsyncSTA(() =>
            {
#if MONO
                using (var form = new FolderBrowserDialog())
#else
                using (var form = new FolderBrowserDialogEx())
#endif
                {
                    form.Description = description;
                    form.SelectedPath = selectedPath;
                    form.ShowNewFolderButton = showNewFolderButton;

                    using (new CrossThreadChecksWaiver())
                    {
                        var r = form.ShowDialog(owner);
                        return r == DialogResult.OK ? form.SelectedPath : null;
                    }

                }
            });

        }






    }
}
