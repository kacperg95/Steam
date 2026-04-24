using DTO.Quiz;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace QuizEngine
{
    public class RoomManager(IServiceScopeFactory _scopeFactory)
    {
        private readonly ConcurrentDictionary<string, RoomDto> _rooms = new();

        public RoomDto GetRoom(string code)
        {
            var room = _rooms[code];
            ArgumentNullException.ThrowIfNull(room);
            return room;
        }

        public string CreateRoom(string connectionId, string nickname, string difficulty)
        {
            var code = Guid.NewGuid().ToString("N")[..6].ToUpper();
            _rooms[code] = new RoomDto
            {
                RoomCode = code,
                Connectionid = connectionId,
                Players = new List<PlayerDto> { new(connectionId, nickname) },
                Difficulty = Enum.Parse<RoomDifficultyDto>(difficulty)
            };
            return code;
        }

        public void RemoveRoom(string roomCode) => _rooms.TryRemove(roomCode, out var room);

        public void AddPlayer(string roomCode, PlayerDto player)
        {
            var room = GetRoom(roomCode);
            room.Players.Add(player);
        }

        public void RemovePlayer(string roomCode, PlayerDto player)
        {
            var room = GetRoom(roomCode);
            room.Players.Remove(player);
            if (room.Players.Count == 0) RemoveRoom(roomCode);
        }

        public (RoomDto?, PlayerDto?) RemovePlayer(string connectionId)
        {
            var room = _rooms.Values.FirstOrDefault(r => r.Players.Any(p => p.ConnectionId == connectionId));
            if (room is null) return (null, null);
            var player = room.Players.First(p => p.ConnectionId == connectionId);
            RemovePlayer(room.RoomCode, player);
            return (room, player);
        }

        public void UpdateRoomSettings(string roomCode, string difficulty, int winScore)
        {
            var room = GetRoom(roomCode);
            if (Enum.TryParse<RoomDifficultyDto>(difficulty, out var parsed))
                room.Difficulty = parsed;
            room.WinScore = winScore;
        }

        public void StartQuiz(string roomCode)
        {
            var room = GetRoom(roomCode);
            room.State = RoomStateDto.InGame;
        }

        public async Task<RoomDto> SetNewQuestion(string roomCode)
        {
            var room = GetRoom(roomCode);
            using var scope = _scopeFactory.CreateScope();
            var questionFactory = scope.ServiceProvider.GetRequiredService<QuestionFactory>();
            room.CurrentQuestion = await questionFactory.GetRandomQuestionAsync(room.Difficulty);
            foreach(var player in room.Players)
            {
                player.AnswerIndex = -1;
                player.AnswerTime = null;
                player.IsAnswerCorrect = null;
                player.AnsweredFirst = null;
            }

            return room;
        }

        public bool RegisterAnswer(string roomCode, string connectionId, string answerContent)
        {
            var room = GetRoom(roomCode);
            var player = room.Players.First(p => p.ConnectionId == connectionId);
            if (room.CurrentQuestion is null) return false;
            player.AnswerIndex = room.CurrentQuestion.Answers.IndexOf(room.CurrentQuestion.Answers.First(a => a.Content == answerContent));
            player.AnswerTime = DateTime.UtcNow;
            return room.Players.Select(p => p.AnswerIndex).All(i => i != -1);
        }

        public bool CalculateAnswers(string roomCode)
        {
            var room = GetRoom(roomCode);
            ArgumentNullException.ThrowIfNull(room.CurrentQuestion);

            int correctIndex = room.CurrentQuestion.Answers.FindIndex(x => x.IsValid);

            foreach (var player in room.Players)
            {
                player.IsAnswerCorrect = player.AnswerIndex == correctIndex;
                if (player.IsAnswerCorrect == true)
                    player.Score++;
            }

            var firstPlayer = room.Players.Where(p => p.IsAnswerCorrect == true)
                .OrderBy(p => p.AnswerTime)
                .FirstOrDefault();

            if (firstPlayer != null && room.Players.Count > 1)
            {
                firstPlayer.AnsweredFirst = true;
                firstPlayer.Score++;
            }

            var players = room.Players;
            int highestScore = players.Max(x => x.Score);

            return highestScore >= room.WinScore &&
                   players.Count(x => x.Score == highestScore) == 1;
        }

        public PlayerDto FinalizeQuiz(string roomCode)
        {
            var room = GetRoom(roomCode);
            ArgumentNullException.ThrowIfNull(room.Players);   
            PlayerDto winner = room.Players.First(x => x.Score == room.Players.Max(x => x.Score));
            room.State = RoomStateDto.Finished;
            return winner;
        }

        public void BackToLobby(string roomCode)
        {
            var room = GetRoom(roomCode);
            room.State = RoomStateDto.Lobby;
            foreach (var player in room.Players)
            {
                player.Score = 0;
                player.AnswerIndex = -1;
            }
        }
    }
}
