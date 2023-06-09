﻿using System;

namespace Assets.Scripts.IO
{
    [Serializable]
    public class GameData
    {
        public int waveNumber;
        public int totalMoney;
        public int totalEscaped;
        public int roundEscaped;
        public int totalKilled;
        public int whichEnemiesToSpawn;
        public int enemiesToSpawn;
        public gameStatus currentState;
        public int totalEnemies;
        public int enemiesPerSpawn;
        public float spawnDelay;
        public float[] enemySpawnRates;
        public Enemy03[] enemyList;
    }
}
