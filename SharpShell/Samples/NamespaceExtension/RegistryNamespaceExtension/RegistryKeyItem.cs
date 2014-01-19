using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using SharpShell.Pidl;
using SharpShell.SharpNamespaceExtension;

namespace RegistryNamespaceExtension
{
    public class RegistryKeyItem : IShellNamespaceFolder
    {
        private readonly RegistryKey hiveKey;
        private readonly string displayName;

        public RegistryKeyItem(RegistryKey hiveKey, string displayName)
        {
            this.hiveKey = hiveKey;
            this.displayName = displayName;
            lazyChildKeys = new Lazy<List<RegistryKeyItem>>(() => 
                hiveKey.GetSubKeyNames().ToList().Select(subKeyName => 
                    new RegistryKeyItem(hiveKey.OpenSubKey(subKeyName), subKeyName)).ToList());
            lazyAttributes = new Lazy<List<KeyAttribute>>(() =>
                hiveKey.GetValueNames().Select(valueName => 
                    new KeyAttribute(valueName, hiveKey.GetValue(valueName).ToString())).ToList());
        }

        ShellId IShellNamespaceItem.GetShellId()
        {
            return ShellId.FromString(displayName);
        }

        string IShellNamespaceItem.GetDisplayName(DisplayNameContext displayNameContext)
        {
            return displayName;
        }

        AttributeFlags IShellNamespaceItem.GetAttributes()
        {
            return AttributeFlags.IsFolder | AttributeFlags.MayContainSubFolders;
        }

        IEnumerable<IShellNamespaceItem> IShellNamespaceFolder.GetChildren(ShellNamespaceEnumerationFlags flags)
        {
            //  If we've been asked for folders, return all subkeys.
            if (flags.HasFlag(ShellNamespaceEnumerationFlags.Folders))
            {
                foreach (var childKey in lazyChildKeys.Value)
                    yield return childKey;
            }

            //  If we've been asked for items, return all items.
            if (flags.HasFlag(ShellNamespaceEnumerationFlags.Items))
            {
                foreach (var childAttribute in lazyAttributes.Value)
                    yield return childAttribute;
            }
        }

        public ShellNamespaceFolderView GetView()
        {
            return new DefaultNamespaceFolderView(new[] { new ShellDetailColumn("Name"), new ShellDetailColumn("Value") });
        }

        private readonly Lazy<List<RegistryKeyItem>> lazyChildKeys;
        private readonly Lazy<List<KeyAttribute>> lazyAttributes;
    }
}