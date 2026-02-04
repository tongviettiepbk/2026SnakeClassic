// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("Kj3GLb0K4SsrvotWGdw/1pjPGp9zXUf3jNJjiB+T7332MPCRgeq/Boi0A2gipYu11bo83Lg5t0M88Hwlznz/3M7z+PfUeLZ4CfP////7/v18//H+znz/9Px8///+N0Cm/l/wiJkBXLQDVfmwRyTG1p3suS3CmKJ9mO0YzsOFd7EgPHIGWx6Qm26JqpaZg3avlTQwsIZ8aXJVCKs5U2sXUaPvDUPnDIAcdq+h0GglBJMQnRjbG6drpaGX12p7gHR8gj/gap6Xb1LitzG+7yOHWYQcFnNxUmIrYql4bsQ7CHVCVfttvKKPecoRjLZ4yQY5sytLyDnHsU3Ra+qP39LUYXRHXi7II3bTKvQstT0Qk0mlxqd2tXkc4aM1nO9VtMagKfz9//7/");
        private static int[] order = new int[] { 6,4,8,6,4,13,7,10,9,9,10,12,12,13,14 };
        private static int key = 254;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
