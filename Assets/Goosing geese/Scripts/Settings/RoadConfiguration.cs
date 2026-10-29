using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GoosingGeese.Roads;
using UnityEngine;

namespace GoosingGeese.Settings
{
    public sealed class RoadConfiguration
    {
        private readonly string _sourceJson;

        public RoadConfiguration(RoadSettings settings, int? roadPieceCount = null)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _sourceJson = JsonUtility.ToJson(settings);
            RoadPieceCount = roadPieceCount ?? settings.RoadPieceCount;
            MaxGenerationAttempts = settings.MaxGenerationAttempts;
            FinishRunoutPieces = settings.FinishRunoutPieces;
            CurveSamplesPerSegment = settings.CurveSamplesPerSegment;
            ShowPathLine = settings.ShowPathLine;
            PathLineWidth = settings.PathLineWidth;
            PathLineHeight = settings.PathLineHeight;
            DrawPathGizmos = settings.DrawPathGizmos;
            WaypointGizmoRadius = settings.WaypointGizmoRadius;
            AvailablePieceTypes = Array.AsReadOnly(settings.AvailablePieceTypes == null ? new RoadPieceType[0] : (RoadPieceType[])settings.AvailablePieceTypes.Clone());
            var prefabs = new Dictionary<RoadPieceType, RoadPiece>();

            if (settings.RoadPieceCount < 1 || RoadPieceCount < 1 || FinishRunoutPieces < 1 || CurveSamplesPerSegment < 1 || MaxGenerationAttempts < 1)
                throw new ArgumentException("Road length, finish runout, generation attempts and sample count must be positive.");

            if (settings.PiecePrefabs == null)
                throw new ArgumentException("Road prefabs must be assigned.");

            foreach (RoadPiecePrefabEntry entry in settings.PiecePrefabs)
            {
                if (entry == null || entry.Prefab == null || prefabs.ContainsKey(entry.Type))
                    throw new ArgumentException("Road prefab entries must be unique and assigned.");

                prefabs.Add(entry.Type, entry.Prefab);
            }

            PiecePrefabs = new ReadOnlyDictionary<RoadPieceType, RoadPiece>(prefabs);
        }

        public int RoadPieceCount { get; }
        public int MaxGenerationAttempts { get; }
        public int FinishRunoutPieces { get; }
        public int CurveSamplesPerSegment { get; }
        public bool ShowPathLine { get; }
        public float PathLineWidth { get; }
        public float PathLineHeight { get; }
        public bool DrawPathGizmos { get; }
        public float WaypointGizmoRadius { get; }
        public IReadOnlyList<RoadPieceType> AvailablePieceTypes { get; }
        public IReadOnlyDictionary<RoadPieceType, RoadPiece> PiecePrefabs { get; }

        internal string ToJson()
        {
            return _sourceJson;
        }
    }
}
