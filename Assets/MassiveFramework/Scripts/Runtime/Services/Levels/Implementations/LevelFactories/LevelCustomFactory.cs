﻿using UnityEngine;
using Zenject;

namespace MassiveCore.Framework.Runtime
{
    public class LevelCustomFactory : IFactory<int, Transform, Level>
    {
        private readonly DiContainer _diContainer;
        private readonly IConfigs _configs;

        public LevelCustomFactory(DiContainer diContainer, IConfigs configs)
        {
            _diContainer = diContainer;
            _configs = configs;
        }

        public Level Create(int index, Transform root)
        {
            var configs = _configs.Config<LevelsConfig>().Configs;
            var prefab = configs[index].Prefab;
            var level = _diContainer.InstantiatePrefabForComponent<Level>(prefab, root);
            level.name = prefab.name;
            return level;
        }
    }
}
