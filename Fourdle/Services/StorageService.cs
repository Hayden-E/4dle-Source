using Microsoft.JSInterop;

namespace Fourdle.Services
{
    public class StorageService
    {
        public IJSRuntime _js;

        public StorageService(IJSRuntime jSRuntime)
        {
            _js = jSRuntime;
        }

        public async Task<string[]> TodaysGuesses()
        {
            var guesses = await _js.InvokeAsync<string>("localStorage.getItem", "todays-guesses");

            if (string.IsNullOrEmpty(guesses))
                return new string[0];

            string[] result = guesses.Split(',');

            if (result[0] == DateTime.Now.ToString("yyyy-MM-dd"))
                return result.Skip(1).ToArray();
            else
                return new string[0];
        }

        public async Task SaveGuesses(string[] guesses)
        {
            string[] newGuesses = new string[guesses.Length + 1];
            newGuesses[0] = DateTime.Now.ToString("yyyy-MM-dd");
            Array.Copy(guesses, 0, newGuesses, 1, guesses.Length);
            await _js.InvokeVoidAsync("localStorage.setItem", "todays-guesses", string.Join(',', newGuesses));
        }

        public async Task<int> GetCurrentStreak()
        {
            var streak = await _js.InvokeAsync<string>("localStorage.getItem", "current-streak");
            if (string.IsNullOrEmpty(streak))
                return 0;
            return int.Parse(streak);
        }

        public async Task AddToStreak()
        {
            int currentStreak = await GetCurrentStreak();
            currentStreak++;
            await _js.InvokeVoidAsync("localStorage.setItem", "current-streak", currentStreak.ToString());
        }

        public async Task ResetStreak()
        {
            await _js.InvokeVoidAsync("localStorage.setItem", "current-streak", 0.ToString());
        }
    }
}
