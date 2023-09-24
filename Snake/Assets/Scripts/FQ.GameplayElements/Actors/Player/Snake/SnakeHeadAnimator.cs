﻿using System.Linq;
using FQ.GameElementCommunication;
using FQ.GameObjectPromises;
using FQ.Logger;
using UnityEngine;

namespace FQ.GameplayElements
{
    /// <summary>
    /// Manages animation for the Snake Player Head.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class SnakeHeadAnimator : GameElement, ISnakeHeadAnimator
    {
        /// <summary>
        /// Provides the correct details and methods to control the animator.
        /// </summary>
        private ISnakeHeadAnimationBehaviour behaviour;

        /// <summary>
        /// Animator on the Object.
        /// The goal of this object is to update the animator based on the behaviour.
        /// </summary>
        private Animator animator;

        /// <summary>
        /// The name of the direction parameter.
        /// </summary>
        private const string DirectionParam = "Direction";
        
        /// <summary>
        /// Start is called on the frame when a script is enabled just
        /// before any of the Update methods are called the first time.
        /// </summary>
        protected override void BaseStart()
        {
            IElementCommunication communication = AcquireElementCommunication();
            IPlayerStatusBasics playerStatus = communication.PlayerStatus;
            if (playerStatus == null)
            {
                Log.Error($"{typeof(SnakePlayer)}: Unable to find {nameof(IPlayerStatusBasics)}." +
                          $"Animations on the player will no longer occur.");
                return;
            }
            
            ICollectableStatusBasics collectableStatus = communication.CollectableStatus;
            if (collectableStatus == null)
            {
                Log.Error($"{typeof(SnakePlayer)}: Unable to find {nameof(ICollectableStatusBasics)}." +
                          $"Animations on the player will no longer occur.");
                return;
            }

            IWorldInfoFromTilemap worldInfo = AcquireWorldInfo();
            if (worldInfo == null)
            {
                Log.Error($"{typeof(SnakePlayer)}: Unable to find {nameof(worldInfo)}." +
                          $"Animations on the player will no longer occur.");
                return;
            }

            this.animator = GetComponent<Animator>();
            
            this.behaviour = new SnakeHeadAnimationBehaviour(playerStatus, worldInfo, collectableStatus);
            this.behaviour.ParamDirection += OnUpdateDirectionParameter;
        }

        /// <summary>
        /// Occurs when the behaviour has a new direction to push.
        /// </summary>
        /// <param name="newDirection">The new direction to set in the Animator</param>
        private void OnUpdateDirectionParameter(int newDirection)
        {
            var parameter = this.animator.parameters.First(x => x.name == DirectionParam);
            this.animator.SetInteger(parameter.nameHash, newDirection);
        }

        /// <summary>
        /// Acquires the <see cref="IElementCommunication"/> from the scene.
        /// </summary>
        /// <returns>A <see cref="IElementCommunication"/> or null if none exists. </returns>
        private IElementCommunication AcquireElementCommunication()
        {
            IElementCommunicationFinder elementCommunicationFinder = new ElementCommunicationFinder();
            IElementCommunication communication = elementCommunicationFinder.FindElementCommunication();
            if (communication == null)
            {
                Log.Error($"{typeof(SnakePlayer)}: No {nameof(IElementCommunication)}. Cannot find player status.");
                return null;
            }

            return communication;
        }
        
        /// <summary>
        /// Finds and returns information on the world map if in the scene.
        /// </summary>
        /// <returns><see cref="IWorldInfoFromTilemap"/> if in the scene or null if not.</returns>
        private IWorldInfoFromTilemap AcquireWorldInfo()
        {
            IWorldInfoFromTilemapFinder finder = new SnakeWorldInfoFromTilemapFinder();
            return finder.FindWorldInfo();
        }
    }
}