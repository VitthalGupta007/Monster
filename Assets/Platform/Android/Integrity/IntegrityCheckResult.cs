namespace VXMonster.Platform.Integrity
{
    public readonly struct IntegrityCheckResult
    {
        public bool Success { get; }
        public string Token { get; }
        public string Error { get; }

        public IntegrityCheckResult(bool success, string token, string error)
        {
            Success = success;
            Token = token;
            Error = error;
        }

        public static IntegrityCheckResult Succeeded(string token) =>
            new IntegrityCheckResult(true, token, null);

        public static IntegrityCheckResult Failed(string error) =>
            new IntegrityCheckResult(false, null, error);
    }
}
