using DTO.Quiz;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using QuizEngine;
using System.Collections.Generic;

namespace API.Hubs
{
    public class QuizHub(RoomManager _roomManager, ILogger<QuizHub> _logger) : Hub
    {
        private IDisposable? BeginRoomScope(string roomId) => _logger.BeginScope(new Dictionary<string, object?> { ["RoomId"] = roomId });

        public async Task CreateRoom(string nickname)
        {
            var roomCode = _roomManager.CreateRoom(Context.ConnectionId, nickname, RoomDifficultyDto.Medium.ToString());
            using var scope = BeginRoomScope(roomCode);
            _logger.LogInformation("Room created {RoomId} by {Nickname}", roomCode, nickname);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.Caller.SendAsync("RoomUpdated", _roomManager.GetRoom(roomCode));
        }

        public async Task JoinRoom(string roomCode, string nickname)
        {
            PlayerDto player = new(Context.ConnectionId, nickname);
            _roomManager.AddPlayer(roomCode, player);
            using var scope = BeginRoomScope(roomCode);
            _logger.LogInformation("Player {Nickname} added to room {RoomId}", nickname, roomCode);
            await Groups.AddToGroupAsync(Context.ConnectionId, roomCode);
            await Clients.GroupExcept(roomCode, Context.ConnectionId).SendAsync("PlayerJoined", player);
            await Clients.Caller.SendAsync("RoomUpdated", _roomManager.GetRoom(roomCode));
        }

        public async Task UpdateRoomSettings(string roomCode, string difficulty, int winScore)
        {
            using var scope = BeginRoomScope(roomCode);
            _logger.LogInformation("UpdateRoomSettings for {RoomId}: difficulty={Difficulty}, winScore={WinScore}", roomCode, difficulty, winScore);
            _roomManager.UpdateRoomSettings(roomCode, difficulty, winScore);
            await Clients.GroupExcept(roomCode, Context.ConnectionId).SendAsync("RoomUpdated", _roomManager.GetRoom(roomCode));
        }

        public async Task StartGame(string roomCode)
        {
            _roomManager.StartQuiz(roomCode);
            using var scope = BeginRoomScope(roomCode);
            _logger.LogInformation("Game started for {RoomId}", roomCode);
            await Clients.Group(roomCode).SendAsync("GameStarted", "");
            await NextQuestion(roomCode);
        }
        public async Task NextQuestion(string roomCode)
        {   
            var room = await _roomManager.SetNewQuestion(roomCode);
            using var scope = BeginRoomScope(roomCode);
            _logger.LogInformation("New question set for {RoomId}", roomCode);
            await Clients.Group(roomCode).SendAsync("NewQuestion", room);
        }

        public async Task SubmitAnswer(string roomCode, string answerContent)
        {
            using var scope = BeginRoomScope(roomCode);
            _logger.LogInformation("SubmitAnswer for {RoomId}: {Answer}", roomCode, answerContent);
            var allAnswered = _roomManager.RegisterAnswer(roomCode, Context.ConnectionId, answerContent);
            await Clients.Group(roomCode).SendAsync("RoomUpdated", _roomManager.GetRoom(roomCode));

            if (allAnswered)
            {
                _logger.LogInformation("All players answered in {RoomId}", roomCode);
                await FinishQuestion(roomCode);
            }
        }

        public async Task FinishQuestion(string roomCode)
        {
            using var scope = BeginRoomScope(roomCode);
            _logger.LogInformation("FinishQuestion invoked for {RoomId}", roomCode);
            var room = _roomManager.GetRoom(roomCode);
            if (room.CurrentQuestion == null)
            {
                _logger.LogWarning("FinishQuestion called but no current question for {RoomId}", roomCode);
                return;
            }

            var maxScoreAchieved = _roomManager.CalculateAnswers(roomCode);
            _logger.LogInformation("Question finished for {RoomId}. MaxScoreAchieved={Max}", roomCode, maxScoreAchieved);
            await Clients.Group(roomCode).SendAsync("QuestionFinished", room.Players);
            room.CurrentQuestion = null;

            if (maxScoreAchieved)
            {
                var winner = _roomManager.FinalizeQuiz(roomCode);
                _logger.LogInformation("Game finished for {RoomId}. Winner={Winner}", roomCode, winner.Name);
                await Clients.Group(roomCode).SendAsync("GameFinished", winner.Name);
            }
        }

        public async Task BackToLobby(string roomCode)
        {
            using var scope = BeginRoomScope(roomCode);
            _logger.LogInformation("BackToLobby for {RoomId} requested", roomCode);
            _roomManager.BackToLobby(roomCode);
            await Clients.Group(roomCode).SendAsync("RoomUpdated", _roomManager.GetRoom(roomCode));
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var (room, player) = _roomManager.RemovePlayer(Context.ConnectionId);
            if (room != null && player != null)
            {
                using var scope = BeginRoomScope(room.RoomCode);
                _logger.LogInformation("Player {Player} disconnected and removed from {RoomId}", player.Name, room.RoomCode);
                await Clients.Group(room.RoomCode).SendAsync("PlayerLeft", player);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
