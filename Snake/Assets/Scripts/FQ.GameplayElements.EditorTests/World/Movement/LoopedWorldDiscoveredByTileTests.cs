﻿using System;
using System.Collections.Generic;
using System.Linq;
using FQ.Libraries;
using FQ.Libraries.StandardTypes;
using FQ.Logger;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FQ.GameplayElements.EditorTests
{
    public class LoopedWorldDiscoveredByTileTests
    {
        private const string TestGridLocation = "TestResources/World/TestGrid";
        private const string TestGridWithTilesTouchingLocation = "TestResources/World/TestGrid-AllTouching";
        private const string TestGridWithTiles1BorderApartLocation =  "TestResources/World/TestGrid-CloseBy1";
        private const string TestGridWithTiles2BordersApartLocation = "TestResources/World/TestGrid-CloseBy2";
        private const string BorderTileLocation = "TestResources/World/TestBasicChecker-A/TestBasicChecker-A-Tile";

        private LoopedWorldDiscoveredByTile testClass;

        [SetUp]
        public void Setup()
        {
            Log.TestMode();
            this.testClass = new LoopedWorldDiscoveredByTile();
        }

        [TearDown]
        public void Teardown()
        {
            Log.ResetTestMode();
        }
        
        #region CalculateLoops

        [Test]
        public void CalculateLoops_ReturnsFalse_WhenGivenTilemapIsNullTest()
        {
            // Arrange
            Tilemap testTilemap = null;
            var borderTile = Resources.Load<Tile>(BorderTileLocation);
            
            // Act
            bool actual = this.testClass.CalculateLoops(testTilemap, borderTile, out _);
            
            // Assert
            Assert.IsFalse(actual);
        }

        [Test]
        public void CalculateLoops_ReturnsTrue_WhenGivenAMapWithBorderTest()
        {
            // Arrange
            Tilemap testTilemap = GetTestBorderTileMap(TestGridLocation);
            var borderTile = Resources.Load<Tile>(BorderTileLocation);
            
            // Act
            bool actual = this.testClass.CalculateLoops(testTilemap, borderTile, out _);
            
            // Assert
            Assert.IsTrue(actual);
        }

        [Test]
        public void CalculateLoops_ReturnsBorderLocations_WhenGivenAMapWithBorderTest()
        {
            // Arrange
            Tilemap testTilemap = GetTestBorderTileMap(TestGridLocation);
            var borderTile = Resources.Load<Tile>(BorderTileLocation);
            List<Vector2Int> expectedLocations = GetBorderLocationInTestMap();
            
            // Act
            this.testClass.CalculateLoops(testTilemap, borderTile,
                out Dictionary<Vector2Int, Dictionary<MovementDirection, CollisionPositionAnswer>> actual);
            
            // Assert
            List<Vector2Int> actualCompare = actual.Keys.ToList();
            AssertCollectionEqualWithBetterFeedback(expectedLocations, actualCompare);
        }

        [Test]
        public void CalculateLoops_ReturnsCorrectDirections_WhenGivenAMapWithBordersTest()
        {
            // Arrange
            Tilemap testTilemap = GetTestBorderTileMap(TestGridLocation);
            var borderTile = Resources.Load<Tile>(BorderTileLocation);
            var expectedLocations = new Dictionary<Vector2Int, Dictionary<MovementDirection, CollisionPositionAnswer>>
            {
                // Left side
                {new Vector2Int(-3, -3), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(-3, -2), MakeSimpleLoopAnswer(MovementDirection.Left, 1, -2)},
                {new Vector2Int(-3, -1), MakeSimpleLoopAnswer(MovementDirection.Left, 1, -1)},
                {new Vector2Int(-3, 0), MakeSimpleLoopAnswer(MovementDirection.Left, 1, 0)},
                {new Vector2Int(-3, 1), MakeSimpleLoopAnswer(MovementDirection.Left, 1, 1)},
                {new Vector2Int(-3, 2), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                
                // Right side
                {new Vector2Int(2, -3), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(2, -2), MakeSimpleLoopAnswer(MovementDirection.Right, -2, -2)},
                {new Vector2Int(2, -1), MakeSimpleLoopAnswer(MovementDirection.Right, -2, -1)},
                {new Vector2Int(2, 0), MakeSimpleLoopAnswer(MovementDirection.Right, -2, 0)},
                {new Vector2Int(2, 1), MakeSimpleLoopAnswer(MovementDirection.Right, -2, 1)},
                {new Vector2Int(2, 2), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                
                // Top side
                {new Vector2Int(-2, -3), MakeSimpleLoopAnswer(MovementDirection.Up, -2, 1)},
                {new Vector2Int(-1, -3), MakeSimpleLoopAnswer(MovementDirection.Up, -1, 1)},
                {new Vector2Int(0, -3), MakeSimpleLoopAnswer(MovementDirection.Up, 0, 1)},
                {new Vector2Int(1, -3), MakeSimpleLoopAnswer(MovementDirection.Up, 1, 1)},
                
                // Bottom side
                {new Vector2Int(-2, 2), MakeSimpleLoopAnswer(MovementDirection.Down, -2, -2)},
                {new Vector2Int(-1, 2), MakeSimpleLoopAnswer(MovementDirection.Down, -1, -2)},
                {new Vector2Int(0, 2), MakeSimpleLoopAnswer(MovementDirection.Down, 0, -2)},
                {new Vector2Int(1, 2), MakeSimpleLoopAnswer(MovementDirection.Down, 1, -2)}
            };
            
            // Act
            this.testClass.CalculateLoops(testTilemap, borderTile,
                out Dictionary<Vector2Int, Dictionary<MovementDirection, CollisionPositionAnswer>> actual);
            
            // Assert
            Assert.AreEqual(expectedLocations.Keys.Count, actual.Keys.Count);
            foreach (var expectedLocation in expectedLocations)
            {
                // Check the keys in the first dictionary
                Assert.IsTrue(actual.ContainsKey(expectedLocation.Key), $"Does not contain {expectedLocation.Key}");
                actual.TryGetValue(expectedLocation.Key, 
                    out Dictionary<MovementDirection, CollisionPositionAnswer> actualLocationValue);
                Assert.NotNull(actualLocationValue, $"actualValue is null. {actualLocationValue}");
                Assert.AreEqual(expectedLocation.Value.Keys.Count, actualLocationValue.Keys.Count,
                    $"Direction dictionary incorrectly sized for {expectedLocation.Key}");
                
                foreach (var expectedValue in expectedLocation.Value)
                {
                    if (expectedValue.Value.Answer == ContextToPositionAnswer.NoValidMovement)
                    {
                        continue;
                    }
                    
                    // Check the value in the first dictionary
                    Assert.IsTrue(actualLocationValue.ContainsKey(expectedValue.Key),
                        $"Does not contain direction {expectedLocation.Key}.{expectedValue.Key}");
                    actualLocationValue.TryGetValue(expectedValue.Key, 
                        out CollisionPositionAnswer actualCollisionAnswer);
                    Assert.NotNull(actualCollisionAnswer, 
                        "actualCollisionAnswer is null meaning direction exists but object does not.");
                    
                    // Check the final bit in the dictionary
                    Assert.AreEqual(expectedLocation.Key, actualCollisionAnswer.NewDirection,
                        $"Direction in final answer was incorrect. {expectedLocation.Key}.{expectedValue.Key}");
                    Assert.AreEqual(ContextToPositionAnswer.NewPositionIsCorrect, actualCollisionAnswer.Answer,
                        $"Enum Answer in final answer was incorrect. {expectedLocation.Key}.{expectedValue.Key}");
                    Assert.AreEqual(expectedValue.Value.NewPosition, actualCollisionAnswer.NewPosition,
                        $"Position in final answer was incorrect. {expectedLocation.Key}.{expectedValue.Key}");
                }
            }
        }
        
        [Test]
        public void CalculateLoops_ReturnsCorrectDirections_WhenBordersHaveTwoEmptyBlocksTest()
        {
            // Arrange
            Tilemap testTilemap = GetTestBorderTileMap(TestGridWithTiles2BordersApartLocation);
            var borderTile = Resources.Load<Tile>(BorderTileLocation);

            var expected = new Dictionary<Vector2Int, Dictionary<MovementDirection, CollisionPositionAnswer>>
            {
                // Left side
                {new Vector2Int(-3, -1), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(-3, 0), MakeSimpleLoopAnswer(MovementDirection.Left, -3, 0)},
                {new Vector2Int(-3, 1), MakeSimpleLoopAnswer(MovementDirection.Left, -3, 1)},
                {new Vector2Int(-3, 2), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                
                // Right side
                {new Vector2Int(0, -1), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(0, 0), MakeSimpleLoopAnswer(MovementDirection.Right, 0, 0)},
                {new Vector2Int(0, 1), MakeSimpleLoopAnswer(MovementDirection.Right, 0, 1)},
                {new Vector2Int(0, 2), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                
                // Top side
                {new Vector2Int(-2, -1), MakeSimpleLoopAnswer(MovementDirection.Up, -2, -1)},
                {new Vector2Int(-1, -1), MakeSimpleLoopAnswer(MovementDirection.Up, -1, -1)},

                // Bottom side
                {new Vector2Int(-2, 2), MakeSimpleLoopAnswer(MovementDirection.Down, -2, 2)},
                {new Vector2Int(-1, 2), MakeSimpleLoopAnswer(MovementDirection.Down, -1, 2)},
            };
            
            // Act
            this.testClass.CalculateLoops(testTilemap, borderTile,
                out Dictionary<Vector2Int, Dictionary<MovementDirection, CollisionPositionAnswer>> actual);
            
            // Assert
            Assert.AreEqual(expected.Keys.Count, actual.Keys.Count);
            foreach (var expectedLocation in expected)
            {
                // Check the keys in the first dictionary
                Assert.IsTrue(actual.ContainsKey(expectedLocation.Key), $"Does not contain {expectedLocation.Key}");
                actual.TryGetValue(expectedLocation.Key, 
                    out Dictionary<MovementDirection, CollisionPositionAnswer> actualLocationValue);
                Assert.NotNull(actualLocationValue, $"actualValue is null. {actualLocationValue}");
                Assert.AreEqual(expectedLocation.Value.Keys.Count, actualLocationValue.Keys.Count,
                    $"Direction dictionary incorrectly sized for {expectedLocation.Key}");
                
                foreach (var expectedValue in expectedLocation.Value)
                {
                    if (expectedValue.Value.Answer == ContextToPositionAnswer.NoValidMovement)
                    {
                        continue;
                    }
                    
                    // Check the value in the first dictionary
                    Assert.IsTrue(actualLocationValue.ContainsKey(expectedValue.Key),
                        $"Does not contain direction {expectedLocation.Key}.{expectedValue.Key}");
                    actualLocationValue.TryGetValue(expectedValue.Key, 
                        out CollisionPositionAnswer actualCollisionAnswer);
                    Assert.NotNull(actualCollisionAnswer, 
                        "actualCollisionAnswer is null meaning direction exists but object does not.");
                    
                    // Check the final bit in the dictionary
                    Assert.AreEqual(expectedLocation.Key, actualCollisionAnswer.NewDirection,
                        $"Direction in final answer was incorrect. {expectedLocation.Key}.{expectedValue.Key}");
                    Assert.AreEqual(ContextToPositionAnswer.NewPositionIsCorrect, actualCollisionAnswer.Answer,
                        $"Enum Answer in final answer was incorrect. {expectedLocation.Key}.{expectedValue.Key}");
                    Assert.AreEqual(expectedValue.Value.NewPosition, actualCollisionAnswer.NewPosition,
                        $"Position in final answer was incorrect. {expectedLocation.Key}.{expectedValue.Key}");
                }
            }
        }
        
        [Test]
        public void CalculateLoops_DoesNotReturnAnyDirections_WhenBordersHasOneEmptyBlocksTest()
        {
            // Arrange
            Tilemap testTilemap = GetTestBorderTileMap(TestGridWithTiles1BorderApartLocation);
            var borderTile = Resources.Load<Tile>(BorderTileLocation);

            var expected = new Dictionary<Vector2Int, Dictionary<MovementDirection, CollisionPositionAnswer>>
            {
                // Right side
                {new Vector2Int(-2, 0), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(-2, 1), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(-2, 2), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                
                // Left side
                {new Vector2Int(0, 0), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(0, 1), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(0, 2), new Dictionary<MovementDirection, CollisionPositionAnswer>()},

                // Top side
                {new Vector2Int(-1, 0), new Dictionary<MovementDirection, CollisionPositionAnswer>()},

                // Bottom side
                {new Vector2Int(-1, 2), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
            };
            
            // Act
            this.testClass.CalculateLoops(testTilemap, borderTile,
                out Dictionary<Vector2Int, Dictionary<MovementDirection, CollisionPositionAnswer>> actual);
            
            // Assert
            Assert.AreEqual(expected.Keys.Count, actual.Keys.Count);
            foreach (var expectedLocation in expected)
            {
                // Check the keys in the first dictionary
                Assert.IsTrue(actual.ContainsKey(expectedLocation.Key), $"Does not contain {expectedLocation.Key}");
                actual.TryGetValue(expectedLocation.Key, 
                    out Dictionary<MovementDirection, CollisionPositionAnswer> actualLocationValue);
                Assert.NotNull(actualLocationValue, $"actualValue is null. {actualLocationValue}");
                Assert.AreEqual(expectedLocation.Value.Keys.Count, actualLocationValue.Keys.Count,
                    $"Direction dictionary incorrectly sized for {expectedLocation.Key}");
                
                Assert.IsFalse(expectedLocation.Value.Keys.Any(), $"{expectedLocation.Key} had a direction.");
            }
        }
        
        [Test]
        public void CalculateLoops_DoesNotReturnAnyDirections_WhenAllBlocksAreTouchingTest()
        {
            // Arrange
            Tilemap testTilemap = GetTestBorderTileMap(TestGridWithTilesTouchingLocation);
            var borderTile = Resources.Load<Tile>(BorderTileLocation);

            var expected = new Dictionary<Vector2Int, Dictionary<MovementDirection, CollisionPositionAnswer>>
            {
                // Right side
                {new Vector2Int(-2, 0), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(-2, 1), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(-2, 2), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                
                // Left side
                {new Vector2Int(0, 0), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(0, 1), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                {new Vector2Int(0, 2), new Dictionary<MovementDirection, CollisionPositionAnswer>()},

                // Top side
                {new Vector2Int(-1, 0), new Dictionary<MovementDirection, CollisionPositionAnswer>()},

                // Bottom side
                {new Vector2Int(-1, 2), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
                
                // Center
                {new Vector2Int(-1, 1), new Dictionary<MovementDirection, CollisionPositionAnswer>()},
            };
            
            // Act
            this.testClass.CalculateLoops(testTilemap, borderTile,
                out Dictionary<Vector2Int, Dictionary<MovementDirection, CollisionPositionAnswer>> actual);
            
            // Assert
            Assert.AreEqual(expected.Keys.Count, actual.Keys.Count);
            foreach (var expectedLocation in expected)
            {
                // Check the keys in the first dictionary
                Assert.IsTrue(actual.ContainsKey(expectedLocation.Key), $"Does not contain {expectedLocation.Key}");
                actual.TryGetValue(expectedLocation.Key, 
                    out Dictionary<MovementDirection, CollisionPositionAnswer> actualLocationValue);
                Assert.NotNull(actualLocationValue, $"actualValue is null. {actualLocationValue}");
                Assert.AreEqual(expectedLocation.Value.Keys.Count, actualLocationValue.Keys.Count,
                    $"Direction dictionary incorrectly sized for {expectedLocation.Key}");
                
                Assert.IsFalse(expectedLocation.Value.Keys.Any(), $"{expectedLocation.Key} had a direction.");
            }
        }
        
        #endregion
        
        #region FindNewPositionForPlayer
        
        [Test]
        public void FindNewPositionForPlayer_ThrowsInvalidCallOrder_WhenCalledBeforeCalculateTest()
        {
            // Arrange

            // Act Assert
            Assert.Throws<InvalidCallOrder>(() =>
            {
                this.testClass.FindNewPositionForPlayer(new Vector2Int(), MovementDirection.Down);
            });
        }
        
        [Test]
        public void FindNewPositionForPlayer_ThrowsInvalidCallOrder_WhenCalculateCalledWithInvalidParametersTest()
        {
            // Arrange
            this.testClass.CalculateLoops(null, new Tile(), out _);

            // Act Assert
            Assert.Throws<InvalidCallOrder>(() =>
            {
                this.testClass.FindNewPositionForPlayer(new Vector2Int(), MovementDirection.Down);
            });
        }
        
        [Test]
        public void FindNewPositionForPlayer_ReturnsNoValidMovement_WhenCalculateIsCalledFirstTest()
        {
            // Arrange
            var expected = ContextToPositionAnswer.NoValidMovement;
            Tilemap testTilemap = GetTestBorderTileMap(TestGridLocation);
            this.testClass.CalculateLoops(testTilemap, new Tile(), out _);
            
            // Act
            CollisionPositionAnswer actual = this.testClass.FindNewPositionForPlayer(new Vector2Int(), MovementDirection.Down);
            
            // Assert
            Assert.AreEqual(expected, actual.Answer);
        }
        
        [Test]
        public void FindNewPositionForPlayer_ReturnsLoopValuesForLeftSide_WhenCalculatedTestGridTest()
        {
            // Arrange
            var expectedAnswer = ContextToPositionAnswer.NewPositionIsCorrect;
            var expectedLocation = new Vector2Int(1, -1);
            var expectedDirection = MovementDirection.Left;
            
            var givenLocation = new Vector2Int(-3, -1);
            var givenDirection = MovementDirection.Left;
            Tilemap givenTileMap = GetTestBorderTileMap(TestGridLocation);
            var givenBorderTile = Resources.Load<Tile>(BorderTileLocation);
            
            this.testClass.CalculateLoops(givenTileMap, givenBorderTile,
                out Dictionary<Vector2Int, Dictionary<MovementDirection, CollisionPositionAnswer>> loopAnswer);

            // Act
            CollisionPositionAnswer actual = this.testClass.FindNewPositionForPlayer(givenLocation, givenDirection);
            
            // Assert
            Assert.AreEqual(expectedAnswer, actual.Answer);
            Assert.AreEqual(expectedLocation, actual.NewPosition);
            Assert.AreEqual(expectedDirection, actual.NewDirection);
        }
        
        [Test]
        public void FindNewPositionForPlayer_ReturnsNoValidMovementOnLeftSide_WhenCalculatedTestGridButGivenIncorrectDirectionTest()
        {
            // Arrange
            var expectedAnswer = ContextToPositionAnswer.NoValidMovement;
            
            var givenLocation = new Vector2Int(-3, -1);
            var givenDirection = MovementDirection.Right;
            Tilemap givenTileMap = GetTestBorderTileMap(TestGridLocation);
            var givenBorderTile = Resources.Load<Tile>(BorderTileLocation);
            
            this.testClass.CalculateLoops(givenTileMap, givenBorderTile, out _);

            // Act
            CollisionPositionAnswer actual = this.testClass.FindNewPositionForPlayer(givenLocation, givenDirection);
            
            // Assert
            Assert.AreEqual(expectedAnswer, actual.Answer);
        }
        
        [Test]
        public void FindNewPositionForPlayer_ReturnsLoopValuesForRightSide_WhenCalculatedTestGridTest()
        {
            // Arrange
            var expectedAnswer = ContextToPositionAnswer.NewPositionIsCorrect;
            var expectedLocation = new Vector2Int(-2, -1);
            var expectedDirection = MovementDirection.Right;
            
            var givenLocation = new Vector2Int(2, -1);
            var givenDirection = MovementDirection.Right;
            Tilemap givenTileMap = GetTestBorderTileMap(TestGridLocation);
            var givenBorderTile = Resources.Load<Tile>(BorderTileLocation);
            
            this.testClass.CalculateLoops(givenTileMap, givenBorderTile, out _);

            // Act
            CollisionPositionAnswer actual = this.testClass.FindNewPositionForPlayer(givenLocation, givenDirection);
            
            // Assert
            Assert.AreEqual(expectedLocation, actual.NewPosition);
            Assert.AreEqual(expectedAnswer, actual.Answer);
            Assert.AreEqual(expectedDirection, actual.NewDirection);
        }
        
        [Test]
        public void FindNewPositionForPlayer_ReturnsNoValidMovementOnRightSide_WhenCalculatedTestGridButGivenIncorrectDirectionTest()
        {
            // Arrange
            var expectedAnswer = ContextToPositionAnswer.NoValidMovement;
            
            var givenLocation = new Vector2Int(2, 0);
            var givenDirection = MovementDirection.Down;
            Tilemap givenTileMap = GetTestBorderTileMap(TestGridLocation);
            var givenBorderTile = Resources.Load<Tile>(BorderTileLocation);
            
            this.testClass.CalculateLoops(givenTileMap, givenBorderTile, out _);

            // Act
            CollisionPositionAnswer actual = this.testClass.FindNewPositionForPlayer(givenLocation, givenDirection);
            
            // Assert
            Assert.AreEqual(expectedAnswer, actual.Answer);
        }
        
        [Test]
        public void FindNewPositionForPlayer_ReturnsLoopValuesForTopSide_WhenCalculatedTestGridTest()
        {
            // Arrange
            var expectedAnswer = ContextToPositionAnswer.NewPositionIsCorrect;
            var expectedLocation = new Vector2Int(0, 1);
            var expectedDirection = MovementDirection.Up;
            
            var givenLocation = new Vector2Int(0, -3);
            var givenDirection = MovementDirection.Up;
            Tilemap givenTileMap = GetTestBorderTileMap(TestGridLocation);
            var givenBorderTile = Resources.Load<Tile>(BorderTileLocation);
            
            this.testClass.CalculateLoops(givenTileMap, givenBorderTile, out _);

            // Act
            CollisionPositionAnswer actual = this.testClass.FindNewPositionForPlayer(givenLocation, givenDirection);
            
            // Assert
            Assert.AreEqual(expectedLocation, actual.NewPosition);
            Assert.AreEqual(expectedAnswer, actual.Answer);
            Assert.AreEqual(expectedDirection, actual.NewDirection);
        }
        
        [Test]
        public void FindNewPositionForPlayer_ReturnsNoValidMovementOnTopSide_WhenCalculatedTestGridButGivenIncorrectDirectionTest()
        {
            // Arrange
            var expectedAnswer = ContextToPositionAnswer.NoValidMovement;
            
            var givenLocation = new Vector2Int(-1, -3);
            var givenDirection = MovementDirection.Left;
            Tilemap givenTileMap = GetTestBorderTileMap(TestGridLocation);
            var givenBorderTile = Resources.Load<Tile>(BorderTileLocation);
            
            this.testClass.CalculateLoops(givenTileMap, givenBorderTile, out _);

            // Act
            CollisionPositionAnswer actual = this.testClass.FindNewPositionForPlayer(givenLocation, givenDirection);
            
            // Assert
            Assert.AreEqual(expectedAnswer, actual.Answer);
        }
        
        [Test]
        public void FindNewPositionForPlayer_ReturnsLoopValuesForBottomSide_WhenCalculatedTestGridTest()
        {
            // Arrange
            var expectedAnswer = ContextToPositionAnswer.NewPositionIsCorrect;
            var expectedLocation = new Vector2Int(-1, -2);
            var expectedDirection = MovementDirection.Down;
            
            var givenLocation = new Vector2Int(-1, 2);
            var givenDirection = MovementDirection.Down;
            Tilemap givenTileMap = GetTestBorderTileMap(TestGridLocation);
            var givenBorderTile = Resources.Load<Tile>(BorderTileLocation);
            
            this.testClass.CalculateLoops(givenTileMap, givenBorderTile, out _);

            // Act
            CollisionPositionAnswer actual = this.testClass.FindNewPositionForPlayer(givenLocation, givenDirection);
            
            // Assert
            Assert.AreEqual(expectedLocation, actual.NewPosition);
            Assert.AreEqual(expectedAnswer, actual.Answer);
            Assert.AreEqual(expectedDirection, actual.NewDirection);
        }
        
        [Test]
        public void FindNewPositionForPlayer_ReturnsNoValidMovementOnBottomSide_WhenCalculatedTestGridButGivenIncorrectDirectionTest()
        {
            // Arrange
            var expectedAnswer = ContextToPositionAnswer.NoValidMovement;
            
            var givenLocation = new Vector2Int(0, 2);
            var givenDirection = MovementDirection.Right;
            Tilemap givenTileMap = GetTestBorderTileMap(TestGridLocation);
            var givenBorderTile = Resources.Load<Tile>(BorderTileLocation);
            
            this.testClass.CalculateLoops(givenTileMap, givenBorderTile, out _);

            // Act
            CollisionPositionAnswer actual = this.testClass.FindNewPositionForPlayer(givenLocation, givenDirection);
            
            // Assert
            Assert.AreEqual(expectedAnswer, actual.Answer);
        }
        
        #endregion

        #region Helper Methods
        
        private void AssertCollectionEqualWithBetterFeedback<T>(List<T> expected, List<T> actual)
        {
            Assert.AreEqual(expected.Count(), actual.Count());
            foreach (var location in expected)
            {
                Assert.IsTrue(actual.Contains(location), $"{location} not found in actual.");
            }
            
            foreach (var location in actual)
            {
                Assert.IsTrue(expected.Contains(location), $"{location} not found in expected.");
            }
        }

        private Tilemap GetTestBorderTileMap(string map)
        {
            Tilemap returnTilemap = null;
            
            GameObject borderObject = GetTestBorderObject(map);
            if (borderObject != null)
            {
                returnTilemap = borderObject.GetComponent<Tilemap>();
            }

            return returnTilemap;
        }

        private GameObject GetTestBorderObject(string map)
        {
            var gridGameObject = Resources.Load<GameObject>(map);
            for (int i = 0; i < gridGameObject.transform.childCount; ++i)
            {
                Transform child = gridGameObject.transform.GetChild(i);
                if (child.name == "Border")
                {
                    return child.gameObject;
                }
            }

            return null;
        }
        
        private List<Vector2Int> GetBorderLocationInTestMap()
        {
            var testLocations = new List<Vector2Int>();
            for (int y = -3; y < 3; ++y)
            {
                testLocations.Add(new Vector2Int(-3, y));
                testLocations.Add(new Vector2Int(2, y));
            }
            
            for (int x = -2; x <= 1; ++x)
            {
                testLocations.Add(new Vector2Int(x, -3));
                testLocations.Add(new Vector2Int(x, 2));
            }

            return testLocations;
        }

        private Dictionary<MovementDirection, CollisionPositionAnswer> MakeSimpleLoopAnswer(
            MovementDirection direction, int x, int y)
        {
            var answer = new Dictionary<MovementDirection, CollisionPositionAnswer>();
            var collision = new CollisionPositionAnswer()
            {
                Answer = ContextToPositionAnswer.NoValidMovement, 
                NewPosition = new Vector2Int(x, y),
                NewDirection = direction
            };
            answer.Add(direction, collision);

            return answer;
        }
        
        #endregion
    }
}