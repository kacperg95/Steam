using DTO.Quiz;
using QuizEngine.QuestionGenerator;

namespace QuizEngine
{
    public class QuestionFactory(IEnumerable<IQuestionGenerator> generators)
    {
        private readonly List<IQuestionGenerator> _generators = [.. generators];
        private readonly Random _rnd = new();

        public async Task<QuestionDto> GetRandomQuestionAsync(RoomDifficultyDto difficulty)
        {
            if (_generators == null || _generators.Count == 0)
                throw new InvalidOperationException("No question generators are registered.");

            var alreadyTriedGenerators = new HashSet<int>();

            while (alreadyTriedGenerators.Count < _generators.Count)
            {
                int totalWeight = 0;
                for (int i = 0; i < _generators.Count; i++)
                {
                    if (alreadyTriedGenerators.Contains(i)) continue;
                    totalWeight += _generators[i].Weight;
                }
                int roll = _rnd.Next(totalWeight);

                int cumulative = 0;
                int selectedIndex = -1;
                for (int i = 0; i < _generators.Count; i++)
                {
                    if (alreadyTriedGenerators.Contains(i)) continue;

                    cumulative += _generators[i].Weight;
                    if (roll < cumulative)
                    {
                        selectedIndex = i;
                        break;
                    }
                }

                alreadyTriedGenerators.Add(selectedIndex);

                var questionDto = await _generators[selectedIndex].GenerateAsync(difficulty);
                if (questionDto != null)
                    return questionDto;
            }

            throw new ArgumentException("No generator was able to generate next question");
        }
    }
}