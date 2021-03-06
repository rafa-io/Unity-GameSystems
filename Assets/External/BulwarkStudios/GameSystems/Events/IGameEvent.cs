﻿namespace BulwarkStudios.GameSystems.Events {

    public interface IGameEvent {

        /// <summary>
        /// Add a listener
        /// </summary>
        /// <param name="listener"></param>
        void Listen(object listener);

        /// <summary>
        /// Remove a listener
        /// </summary>
        void Unlisten(object listener);

        /// <summary>
        /// Trigger
        /// </summary>
        void Trigger();

    }

}