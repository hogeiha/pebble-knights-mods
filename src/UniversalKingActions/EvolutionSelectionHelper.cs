using System;
using System.Collections;
using System.Reflection;

namespace UniversalKingActions
{
    internal static class EvolutionSelectionHelper
    {
        private static MethodInfo _localPlayerGetter;
        private static MethodInfo _playerIdGetter;
        private static MethodInfo _playerIsAiGetter;
        private static MethodInfo _playerIsKingGetter;
        private static MethodInfo _playerManagerPlayersGetter;
        private static MethodInfo _playerObjectPlayerIdGetter;
        private static MethodInfo _playerObjectPlayerGetter;

        public static bool CanLocalSelectEvolution(int sessionPlayerId)
        {
            try
            {
                var localPlayer = GetLocalPlayer();
                if (localPlayer == null)
                    return false;

                var localPlayerId = GetPlayerId(localPlayer);
                if (localPlayerId == sessionPlayerId)
                    return true;

                if (!IsKing(localPlayer))
                    return false;

                var sessionPlayer = FindPlayerById(sessionPlayerId);
                return sessionPlayer != null && IsAi(sessionPlayer);
            }
            catch
            {
                return false;
            }
        }

        private static object GetLocalPlayer()
        {
            if (_localPlayerGetter == null)
                _localPlayerGetter = TypeFinder.FindMethod("LocalPlayer", "get_Player");

            return _localPlayerGetter == null ? null : _localPlayerGetter.Invoke(null, null);
        }

        private static int GetPlayerId(object player)
        {
            if (_playerIdGetter == null)
                _playerIdGetter = TypeFinder.FindMethod("Player", "get_Id");

            if (_playerIdGetter == null)
                return -1;

            return Convert.ToInt32(_playerIdGetter.Invoke(player, null));
        }

        private static bool IsAi(object player)
        {
            if (_playerIsAiGetter == null)
                _playerIsAiGetter = TypeFinder.FindMethod("Player", "get_IsAI");

            if (_playerIsAiGetter == null)
                return false;

            return Convert.ToBoolean(_playerIsAiGetter.Invoke(player, null));
        }

        private static bool IsKing(object player)
        {
            if (_playerIsKingGetter == null)
                _playerIsKingGetter = TypeFinder.FindMethod("Player", "get_IsKing");

            if (_playerIsKingGetter == null)
                return false;

            return Convert.ToBoolean(_playerIsKingGetter.Invoke(player, null));
        }

        private static object FindPlayerById(int playerId)
        {
            if (_playerManagerPlayersGetter == null)
                _playerManagerPlayersGetter = TypeFinder.FindMethod("PlayerManager", "get_Players");

            var players = _playerManagerPlayersGetter == null
                ? null
                : _playerManagerPlayersGetter.Invoke(null, null) as IEnumerable;
            if (players == null)
                return null;

            foreach (var playerObject in players)
            {
                if (playerObject == null)
                    continue;

                if (GetPlayerObjectId(playerObject) != playerId)
                    continue;

                return GetPlayerFromObject(playerObject);
            }

            return null;
        }

        private static int GetPlayerObjectId(object playerObject)
        {
            if (_playerObjectPlayerIdGetter == null)
                _playerObjectPlayerIdGetter = TypeFinder.FindMethod("PlayerObjectController", "get_PlayerId");

            if (_playerObjectPlayerIdGetter == null)
                return -1;

            return Convert.ToInt32(_playerObjectPlayerIdGetter.Invoke(playerObject, null));
        }

        private static object GetPlayerFromObject(object playerObject)
        {
            if (_playerObjectPlayerGetter == null)
                _playerObjectPlayerGetter = TypeFinder.FindMethod("PlayerObjectController", "get_Player");

            return _playerObjectPlayerGetter == null ? null : _playerObjectPlayerGetter.Invoke(playerObject, null);
        }
    }
}
