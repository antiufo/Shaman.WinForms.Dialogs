using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Windows.Forms;

namespace Shaman.WinForms
{
    /// <summary>Prompts the user to select a folder. This class cannot be inherited.</summary>
    /// <filterpriority>2</filterpriority>
    public sealed class FolderBrowserDialogEx : CommonDialog
    {


        public delegate int BrowseCallbackProc(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData);
        [DllImport("shell32.dll")]
        public static extern int SHGetSpecialFolderLocation(IntPtr hwnd, int csidl, ref IntPtr ppidl);
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class BROWSEINFO
        {
            public IntPtr hwndOwner;
            public IntPtr pidlRoot;
            public IntPtr pszDisplayName;
            public string lpszTitle;
            public int ulFlags;
            public BrowseCallbackProc lpfn;
            public IntPtr lParam;
            public int iImage;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SHBrowseForFolder([In] BROWSEINFO lpbi);
        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);
        [DllImport("ole32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
        internal static extern void CoTaskMemFree(IntPtr pv);
        [Guid("00000002-0000-0000-c000-000000000046"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SuppressUnmanagedCodeSecurity]
        [ComImport]
        public interface IMalloc
        {
            [PreserveSig]
            IntPtr Alloc(int cb);
            [PreserveSig]
            IntPtr Realloc(IntPtr pv, int cb);
            [PreserveSig]
            void Free(IntPtr pv);
            [PreserveSig]
            int GetSize(IntPtr pv);
            [PreserveSig]
            int DidAlloc(IntPtr pv);
            [PreserveSig]
            void HeapMinimize();
        }
        [DllImport("shell32.dll")]
        public static extern int SHGetMalloc([MarshalAs(UnmanagedType.LPArray)] [Out] IMalloc[] ppMalloc);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, int lParam);


        private Environment.SpecialFolder rootFolder;
        private string descriptionText;
        private string selectedPath;
        private bool showNewFolderButton;
        private bool selectedPathNeedsCheck;
        private BrowseCallbackProc callback;
        /// <summary>Occurs when the user clicks the Help button on the dialog box.</summary>
        /// <filterpriority>1</filterpriority>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler HelpRequest
        {
            add
            {
                base.HelpRequest += value;
            }
            remove
            {
                base.HelpRequest -= value;
            }
        }
        /// <summary>Gets or sets a value indicating whether the New Folder button appears in the folder browser dialog box.</summary>
        /// <returns>true if the New Folder button is shown in the dialog box; otherwise, false. The default is true.</returns>
        /// <filterpriority>1</filterpriority>
        public bool ShowNewFolderButton
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.showNewFolderButton;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.showNewFolderButton = value;
            }
        }
        /// <summary>Gets or sets the path selected by the user.</summary>
        /// <returns>The path of the folder first selected in the dialog box or the last folder selected by the user. The default is an empty string ("").</returns>
        /// <filterpriority>1</filterpriority>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public string SelectedPath
        {
            get
            {
                if (this.selectedPath == null || this.selectedPath.Length == 0)
                {
                    return this.selectedPath;
                }
                if (this.selectedPathNeedsCheck)
                {
                    new FileIOPermission(FileIOPermissionAccess.PathDiscovery, this.selectedPath).Demand();
                }
                return this.selectedPath;
            }
            set
            {
                this.selectedPath = ((value == null) ? string.Empty : value);
                this.selectedPathNeedsCheck = false;
            }
        }
        /// <summary>Gets or sets the root folder where the browsing starts from.</summary>
        /// <returns>One of the <see cref="T:System.Environment.SpecialFolder" /> values. The default is Desktop.</returns>
        /// <exception cref="T:System.ComponentModel.InvalidEnumArgumentException">The value assigned is not one of the <see cref="T:System.Environment.SpecialFolder" /> values. </exception>
        /// <filterpriority>1</filterpriority>
        public Environment.SpecialFolder RootFolder
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.rootFolder;
            }
            set
            {
                if (!Enum.IsDefined(typeof(Environment.SpecialFolder), value))
                {
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(Environment.SpecialFolder));
                }
                this.rootFolder = value;
            }
        }
        /// <summary>Gets or sets the descriptive text displayed above the tree view control in the dialog box.</summary>
        /// <returns>The description to display. The default is an empty string ("").</returns>
        /// <filterpriority>1</filterpriority>
        public string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.descriptionText;
            }
            set
            {
                this.descriptionText = ((value == null) ? string.Empty : value);
            }
        }
        /// <summary>Initializes a new instance of the <see cref="T:System.Windows.Forms.FolderBrowserDialogEx" /> class.</summary>
        public FolderBrowserDialogEx()
        {
            this.Reset();
        }
        /// <summary>Resets properties to their default values.</summary>
        /// <filterpriority>1</filterpriority>
        public override void Reset()
        {
            this.rootFolder = Environment.SpecialFolder.Desktop;
            this.descriptionText = string.Empty;
            this.selectedPath = string.Empty;
            this.selectedPathNeedsCheck = false;
            this.showNewFolderButton = true;
        }
        protected override bool RunDialog(IntPtr hWndOwner)
        {
            IntPtr zero = IntPtr.Zero;
            bool result = false;
            SHGetSpecialFolderLocation(hWndOwner, (int)this.rootFolder, ref zero);
            if (zero == IntPtr.Zero)
            {
                SHGetSpecialFolderLocation(hWndOwner, 0, ref zero);
                if (zero == IntPtr.Zero)
                {
                    throw new InvalidOperationException("FolderBrowserDialogExNoRootFolder");
                }
            }
            int num = 64;
            if (!this.showNewFolderButton)
            {
                num += 512;
            }
            if (Control.CheckForIllegalCrossThreadCalls && Application.OleRequired() != ApartmentState.STA)
            {
                throw new ThreadStateException("ThreadMustBeSTA");
            }
            IntPtr intPtr = IntPtr.Zero;
            IntPtr intPtr2 = IntPtr.Zero;
            IntPtr intPtr3 = IntPtr.Zero;
            try
            {

                num |= 0x00000010; // BIF_EDITBOX
                num |= 0x00000040; // BIF_NEWDIALOGSTYLE
                BROWSEINFO bROWSEINFO = new BROWSEINFO();
                intPtr2 = Marshal.AllocHGlobal(260 * Marshal.SystemDefaultCharSize);
                intPtr3 = Marshal.AllocHGlobal(260 * Marshal.SystemDefaultCharSize);
                this.callback = new BrowseCallbackProc(this.FolderBrowserDialogEx_BrowseCallbackProc);
                bROWSEINFO.pidlRoot = zero;
                bROWSEINFO.hwndOwner = hWndOwner;
                bROWSEINFO.pszDisplayName = intPtr2;
                bROWSEINFO.lpszTitle = this.descriptionText;
                bROWSEINFO.ulFlags = num;
                bROWSEINFO.lpfn = this.callback;
                bROWSEINFO.lParam = IntPtr.Zero;
                bROWSEINFO.iImage = 0;
                intPtr = SHBrowseForFolder(bROWSEINFO);
                if (intPtr != IntPtr.Zero)
                {
                    SHGetPathFromIDList(intPtr, intPtr3);
                    this.selectedPathNeedsCheck = true;
                    this.selectedPath = Marshal.PtrToStringAuto(intPtr3);
                    result = true;
                }
            }
            finally
            {
                CoTaskMemFree(zero);
                if (intPtr != IntPtr.Zero)
                {
                    CoTaskMemFree(intPtr);
                }
                if (intPtr3 != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(intPtr3);
                }
                if (intPtr2 != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(intPtr2);
                }
                this.callback = null;
            }
            return result;
        }
        private static IMalloc GetSHMalloc()
        {
            IMalloc[] array = new IMalloc[1];
            SHGetMalloc(array);
            return array[0];
        }
        private int FolderBrowserDialogEx_BrowseCallbackProc(IntPtr hwnd, int msg, IntPtr lParam, IntPtr lpData)
        {
            switch (msg)
            {
                case 1:
                    if (this.selectedPath.Length != 0)
                    {
                        const int BFFM_SETSELECTIONA = 1126;
                        const int BFFM_SETSELECTIONW = 1127;
                        var BFFM_SETSELECTION = Marshal.SystemDefaultCharSize == 1 ? BFFM_SETSELECTIONA : BFFM_SETSELECTIONW;
                        SendMessage(new HandleRef(null, hwnd), BFFM_SETSELECTION, 1, this.selectedPath);
                    }
                    break;
                case 2:
                    if (lParam != IntPtr.Zero)
                    {
                        IntPtr intPtr = Marshal.AllocHGlobal(260 * Marshal.SystemDefaultCharSize);
                        bool flag = SHGetPathFromIDList(lParam, intPtr);
                        Marshal.FreeHGlobal(intPtr);
                        SendMessage(new HandleRef(null, hwnd), 1125, 0, flag ? 1 : 0);
                    }
                    break;
            }
            return 0;
        }
    }
}
