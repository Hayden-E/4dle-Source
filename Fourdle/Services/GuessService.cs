using static System.Net.WebRequestMethods;

namespace Fourdle.Services
{
    public class GuessService
    {
        private readonly HttpClient _http;

        string[] _loadedWords;
        string[] _loadedGuessWords;

        public GuessService(HttpClient http)
        {
            _http = http;
        }

        public int DefaultSeed()
        {
            return (int)((DateTimeOffset.Now.ToUnixTimeSeconds() - 14400) / 86400);
        }

        public async Task<bool> CheckWin(int seed, string input)
        {
            int[] result = await GetGuessResult(seed, input);

            int currentCorrect = 0;

            for (int i = 0; i < result.Length; i++)
                currentCorrect += result[i] == 2 ? 1 : 0;

            return currentCorrect >= 4;
        }

        public async Task<string> GetWordForSeed(int seed)
        {
            if (_loadedWords == null || _loadedWords.Length <= 0)
            {
                string content = await _http.GetStringAsync("content/words.txt");
                string guessContent = await _http.GetStringAsync("content/words_guess.txt");

                // Split by line breaks into an array
                _loadedWords = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                _loadedGuessWords = guessContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }

            Random r = new Random(seed);

            return _loadedWords[r.Next(0, _loadedWords.Length)];
        }

        public async Task<int[]> GetGuessResult(int seed, string guess)
        {
            string currentWord = await GetWordForSeed(seed);

            int[] result = new int[4];

            for (int i = 0; i < 4; i++)
            {
                result[i] = CompareLetters(guess[i], currentWord[i], currentWord, i);

                if (result[i] == 1)
                    result[i] = VerifyLetterType(i, guess, currentWord); // Make sure we don't show more yellows than necessary
            }

            return result;
        }

        public int[] GetGuessResultForWord(string word, string guess)
        {
            int[] result = new int[4];

            for (int i = 0; i < 4; i++)
            {
                result[i] = CompareLetters(guess[i], word[i], word, i);

                if (result[i] == 1)
                    result[i] = VerifyLetterType(i, guess, word); // Make sure we don't show more yellows than necessary
            }

            return result;
        }

        // Verifies and fixes duplicate yellow letters if neccesary
        public int VerifyLetterType(int index, string guess, string word)
        {
            char thisLetter = guess[index];

            int countOfThisLetter = word.Where(x => x == thisLetter).Count();

            // Checking for green letters...
            for (int i = 0; i < guess.Length; i++)
            {
                if (guess[i] == word[i] && guess[i] == thisLetter)
                    countOfThisLetter--;
            }

            // Checking for other yellow letters... if there's multiple yellows then prioritize the left most one
            for (int i = 0; i < guess.Length; i++)
            {
                if (word.Contains(guess[i]) && guess[i] == thisLetter && i < index)
                    countOfThisLetter--;
            }

            // If there's space for a yellow include it
            if (countOfThisLetter > 0)
                return 1;
            else
                return 0;
        }

        public int CompareLetters(char guess, char actual, string full, int index)
        {
            string strAlreadyProcessed = full[..index];

            if (guess == actual)
                return 2;
            else if (full.Contains(guess))
                return 1;
            else
                return 0;
        }

        public async Task<string> GetWordWith(int yellow, int green, string word)
        {
            string result = "";

            if (green == word.Length)
                return word;

            if (_loadedWords == null || _loadedWords.Length <= 0)
            {
                string content = await _http.GetStringAsync("content/words.txt");
                string guessContent = await _http.GetStringAsync("content/words_guess.txt");

                // Split by line breaks into an array
                _loadedWords = content.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                _loadedGuessWords = guessContent.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            }

            int[] typesForWord;

            Random r = new Random(DefaultSeed());

            for (int i = r.Next(0, (int)(_loadedWords.Length * 0.5f)); i < _loadedWords.Length; i++)
            {
                typesForWord = GetGuessResultForWord(_loadedWords[i], word);

                int yellowCount = typesForWord.Where(x => x == 1).Count();
                int greenCount = typesForWord.Where(x => x == 2).Count();

                if (yellowCount == yellow && greenCount == green)
                {
                    result = _loadedWords[i];
                    break;
                }
            }

            return result;
        }

        public bool ValidWord(string word)
        {
            return _loadedGuessWords.Contains(word.ToUpper());
            //return _loadedWords.Contains(word.ToUpper());
        }
    }
}
