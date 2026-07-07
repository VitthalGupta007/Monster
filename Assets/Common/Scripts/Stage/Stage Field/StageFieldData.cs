using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace VXMonster.Core
{
    [CreateAssetMenu(fileName = "Stage Field Data", menuName = "VX Monster/Stage Field Data")]
    public class StageFieldData : ScriptableObject
    {
        [Obsolete("Use backgroundPrefabs field instead")]
        [Tooltip("Obsolete, Use backgroundPrefabs field instead")]
        [FormerlySerializedAs("backgroundPrefab")]
        [SerializeField] protected GameObject obsoleteBackgroundPrefab;

#pragma warning disable 0618
        public GameObject BackgroundPrefab => obsoleteBackgroundPrefab;
#pragma warning restore 0618

        [SerializeField] protected List<GameObject> backgroundPrefabs;
        public List<GameObject> BackgroundPrefabs => backgroundPrefabs;

        [Header("Sides")]
        [SerializeField] protected GameObject topPrefab;
        [SerializeField] protected GameObject bottomPrefab;
        [SerializeField] protected GameObject leftPrefab;
        [SerializeField] protected GameObject rightPrefab;

        public GameObject TopPrefab => topPrefab;
        public GameObject BottomPrefab => bottomPrefab;
        public GameObject LeftPrefab => leftPrefab;
        public GameObject RightPrefab => rightPrefab;

        [Header("Corners")]
        [SerializeField] protected GameObject topRightPrefab;
        [SerializeField] protected GameObject topLeftPrefab;
        [SerializeField] protected GameObject bottomRightPrefab;
        [SerializeField] protected GameObject bottomLeftPrefab;

        public GameObject TopRightPrefab => topRightPrefab;
        public GameObject TopLeftPrefab => topLeftPrefab;
        public GameObject BottomRightPrefab => bottomRightPrefab;
        public GameObject BottomLeftPrefab => bottomLeftPrefab;

        [Header("Margins")]
        [SerializeField] protected float leftMargin = 0;
        [SerializeField] protected float rightMargin = 0;
        [SerializeField] protected float topMargin = 0;
        [SerializeField] protected float bottomMargin = 0;

        public float LeftMargin => leftMargin;
        public float RightMargin => rightMargin;
        public float TopMargin => topMargin;
        public float BottomMargin => bottomMargin;

        [Header("Prop")]
        [SerializeField] protected List<StagePropData> propChances;
        public List<StagePropData> PropChances => propChances;

        public List<GameObject> GetBackgroundPrefabs()
        {
            if (backgroundPrefabs.Count > 0) return backgroundPrefabs;
#pragma warning disable 0618
            return new List<GameObject> { obsoleteBackgroundPrefab };
#pragma warning restore 0618
        }
    }

    [System.Serializable]
    public class StagePropData
    {
        [SerializeField] GameObject prefab;
        [SerializeField, Min(1)] int maxAmount;
        [SerializeField, Range(0, 100)] float chance;
        
        public GameObject Prefab => prefab;
        public int MaxAmount => maxAmount;
        public float Chance => chance;
    }
}