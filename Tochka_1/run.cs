using System;
using System.Collections.Generic;
using System.Linq;

class Run
{
    private static readonly char[] Types = { 'A', 'B', 'C', 'D' };
    private static readonly Dictionary<char, int> Cost = new() { { 'A', 1 }, { 'B', 10 }, { 'C', 100 }, { 'D', 1000 } };
    private static readonly Dictionary<char, int> TargetRoom = new() { { 'A', 0 }, { 'B', 1 }, { 'C', 2 }, { 'D', 3 } };
    private static readonly int[] Entrances = { 2, 4, 6, 8 };
    private static readonly int[] HallwayStops = { 0, 1, 3, 5, 7, 9, 10 };

    class State
    {
        public string Hallway { get; }
        public string[] Rooms { get; }

        public State(string hallway, string[] rooms)
        {
            Hallway = hallway;
            Rooms = rooms;
        }

        public string ToKey()
        {
            return Hallway + "|" + string.Join("|", Rooms);
        }
    }

    static State ParseInput(List<string> lines)
    {
        string hallway = null;
        var letterRows = new List<char[]>();

        foreach (var raw in lines)
        {
            var line = raw;
            if (line.Length >= 13 && line[0] == '#' && line[^1] == '#')
            {
                var core = line.Substring(1, line.Length - 2);
                if (core.Length == 11 && core.All(c => c == '.' || c == 'A' || c == 'B' || c == 'C' || c == 'D'))
                {
                    hallway = core;
                }
            }

            var letters = line.Where(c => c == 'A' || c == 'B' || c == 'C' || c == 'D').ToArray();
            if (letters.Length == 4)
                letterRows.Add(letters);
        }

        hallway ??= new string('.', 11);

        int depth = letterRows.Count;
        var rooms = new string[4];
        for (int r = 0; r < 4; r++)
        {
            var col = new char[depth];
            for (int d = 0; d < depth; d++)
                col[d] = letterRows[d][r];
            rooms[r] = new string(col);
        }

        return new State(hallway, rooms);
    }

    static bool IsGoal(State s)
    {
        if (s.Hallway.Any(c => c != '.')) return false;
        for (int r = 0; r < 4; r++)
        {
            char need = Types[r];
            if (s.Rooms[r].Any(c => c != need)) return false;
        }
        return true;
    }

    static bool ClearHallway(string hallway, int start, int end)
    {
        var step = end > start ? 1 : -1;
        for (var p = start + step; p != end; p += step)
            if (hallway[p] != '.') return false;
        return true;
    }


    static bool RoomReadyFor(char letter, string room)
    {
        return room.All(c => c == '.' || c == letter);
    }

    static int TopOccupantIndex(string room)
    {
        for (int i = 0; i < room.Length; i++)
            if (room[i] != '.') return i;
        return -1;
    }

    static int DeepestEmptyIndex(string room)
    {
        for (int i = room.Length - 1; i >= 0; i--)
            if (room[i] == '.') return i;
        return -1;
    }

    static List<(State state, long cost)> Neighbors(State s)
    {
        var res = new List<(State, long)>();
        var hallway = s.Hallway;
        var rooms = s.Rooms;

        for (int hp = 0; hp < hallway.Length; hp++)
        {
            char who = hallway[hp];
            if (who == '.') continue;
            int targetR = TargetRoom[who];
            int entrance = Entrances[targetR];

            if (!ClearHallway(hallway, hp, entrance)) continue;
            var room = rooms[targetR];
            if (!RoomReadyFor(who, room)) continue;
            int destI = DeepestEmptyIndex(room);
            if (destI == -1) continue;

            int steps = Math.Abs(hp - entrance) + (destI + 1);
            long cost = (long)steps * Cost[who];

            var newHallArr = hallway.ToCharArray();
            newHallArr[hp] = '.';
            var newRoomArr = room.ToCharArray();
            newRoomArr[destI] = who;

            var newRooms = (string[])rooms.Clone();
            newRooms[targetR] = new string(newRoomArr);
            var newState = new State(new string(newHallArr), newRooms);
            res.Add((newState, cost));
        }

        for (int rIndex = 0; rIndex < 4; rIndex++)
        {
            var room = rooms[rIndex];
            int entrance = Entrances[rIndex];

            if (hallway[entrance] != '.') continue;

            int ti = TopOccupantIndex(room);
            if (ti == -1) continue;

            char who = room[ti];

            if (TargetRoom[who] == rIndex)
            {
                bool ok = true;
                for (int k = ti; k < room.Length; k++)
                    if (!(room[k] == '.' || room[k] == who)) { ok = false; break; }
                if (ok) continue;
            }

            foreach (int hp in HallwayStops)
            {
                if (!ClearHallway(hallway, entrance, hp)) continue;
                int steps = (ti + 1) + Math.Abs(hp - entrance);
                long cost = (long)steps * Cost[who];

                var newHallArr = hallway.ToCharArray();
                newHallArr[hp] = who;
                var newRoomArr = room.ToCharArray();
                newRoomArr[ti] = '.';
                var newRooms = (string[])rooms.Clone();
                newRooms[rIndex] = new string(newRoomArr);
                var newState = new State(new string(newHallArr), newRooms);
                res.Add((newState, cost));
            }
        }

        return res;
    }

    static long Dijkstra(State start)
    {
        var pq = new PriorityQueue<State, long>();
        var best = new Dictionary<string, long>();

        pq.Enqueue(start, 0);
        best[start.ToKey()] = 0;

        while (pq.Count > 0)
        {
            pq.TryDequeue(out var state, out var cost);
            string key = state.ToKey();
            if (best.TryGetValue(key, out var known) && cost != known) continue;

            if (IsGoal(state)) return cost;

            foreach (var (nxt, w) in Neighbors(state))
            {
                long nc = cost + w;
                string nk = nxt.ToKey();
                if (!best.TryGetValue(nk, out var old) || nc < old)
                {
                    best[nk] = nc;
                    pq.Enqueue(nxt, nc);
                }
            }
        }

        return long.MaxValue;
    }

    static void Main()
    {
        var lines = new List<string>();
        string line;
        while ((line = Console.ReadLine()) != null)
        {
            if (line.Trim().Length == 0) continue;
            lines.Add(line);
        }

        var start = ParseInput(lines);
        long ans = Dijkstra(start);
        Console.WriteLine(ans);
    }
}