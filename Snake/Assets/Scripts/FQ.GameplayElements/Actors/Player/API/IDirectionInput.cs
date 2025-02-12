﻿using FQ.GameplayInputs;
using FQ.Libraries.StandardTypes;

namespace FQ.GameplayElements
{
    /// <summary>
    /// Handles direction inputs from the input IO.
    /// </summary>
    public interface IDirectionInput
    {
        /// <summary>
        /// Inputs from the user, interfaced behind gameplay meanings.
        /// </summary>
        /// <param name="gameplayInputs">Interaction with Gameplay Inputs. </param>
        /// <exception cref="System.ArgumentNullException"><see cref="IGameplayInputs"/> is null. </exception>
        void Setup(IGameplayInputs gameplayInputs);

        /// <summary>
        /// Determines if the direction is pressed this frame.
        /// </summary>
        /// <param name="direction">Direction to test. </param>
        /// <returns>True means direction is pressed. </returns>
        /// <exception cref="System.Exception">When setup is unsuccessful or not called first. </exception>
        bool PressingInputInDirection(MovementDirection direction);
    }
}