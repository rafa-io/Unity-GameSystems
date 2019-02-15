﻿using UnityEngine;

namespace BulwarkStudios.GameSystems.Contexts {

    public interface IGameContext {

        /// <summary>
        /// Get the scriptable object
        /// </summary>
        /// <returns></returns>
        ScriptableObject GetScriptableObject();

        /// <summary>
        /// Load the context
        /// </summary>
        void Load();

        /// <summary>
        /// Context enabled
        /// </summary>
        /// <param name="index"></param>
        void Enable(GameContextSystem.INDEX index);

        /// <summary>
        /// Context disabled
        /// </summary>
        /// <param name="index"></param>
        void Disable(GameContextSystem.INDEX index);

    }

}
