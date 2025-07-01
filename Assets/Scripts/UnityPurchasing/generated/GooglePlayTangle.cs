// WARNING: Do not modify! Generated file.

using System.Reflection;

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("NOzBPi9rO/NBFyPx+Pp0ezbuxAuIkpe3PZ+zsP6t/Xvt82hBFg0jsoV+AW8RLIAeh0PcMVL8wxZPPcaJnS+sj52gq6SHK+UrWqCsrKyora6WqwqxHzt74qQHZTX6AyEh6zeHMa9zgqgVzzMMTp25DybmFhAowaDPL6yirZ0vrKevL6ysrQ3GQeI3h10vSBMchzOrqs+qFFHPI81mneHUqnzSJXsv4jr7jgaqy8tAEoxyWnYo4A1LllLdPkroHUdA6V7ljm+9NKgjqH9YauCNz70fI46WrJmekyKEc2wr/ZvHrHD05dzLQofg4uyW7/SIFyyYN1FirIOBg7+Rc/c0Im7Ae7T56BHQqCCPwgaT6mdRRjyOmT+iSrqdTiIiz5D5kK+urK2s");
        private static int[] order = new int[] { 3,8,7,3,13,10,8,12,8,10,13,12,13,13,14 };
        private static int key = 173;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
