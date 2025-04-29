using System;
using System.Collections.Generic;
using UnityEngine;
using Bokka.SkinStore;

namespace Bokka.BeachRescue
{
    public class LevelController : MonoBehaviour
    {
        private static LevelController instance;

        public static LevelController Instance
        {
            get => instance;
        }

        public static readonly int WAITING_LEVELS_AMOUNT = 5;

        [Header("Settings")]
        [SerializeField] float minTrailPointsDistance = 0.3f;

        [Header("References")]
        [SerializeField] Camera cameraRef;
        [SerializeField] LayerMask touchLayer;
        [SerializeField] LayerMask boatLayer;
        [SerializeField] LayerMask startPointLayer;

        public static Level CurrentLevel { get; private set; }

        public static float WaitingTimeLength => CurrentLevel.MaxWaitingDuration;

        public static int TotalCoinsPerLevel { get; private set; }

        public static int CoinsPickedAmount { get; private set; }

        public static bool IsPlaying { get; set; }

        private BoatBehaviour boatBehaviour;
        private IPool startPointsPool;
        private IPool charactersPool;
        private IPool movingObstaclePool;
        private IPool pathTrailsPool;
        private IPool movingObstaclePathRendererPool;

        private StartPointBehaviour startPoint;
        private List<CharacterBehaviour> charactersList = new List<CharacterBehaviour>();
        private List<MovingObstacleBehaviour> movingObstaclesList = new List<MovingObstacleBehaviour>();
        private List<IInteractable> interactablesList = new List<IInteractable>();
        private TrailBehaviour mainTrailBehaviourRef;
        private TrailBehaviour secondTrailBehaviourRef;
        private Dictionary<Item, IPool> itemsPoolsDict = new Dictionary<Item, IPool>();
        private UIGame uiGame;
        private UIMainMenu uiMainMenu;

        private Vector3 lastStartPointPosition;
        private Vector3 lastTrailPoint;


        private bool isInputActive;
        private bool resetTrail;
        private bool skipTouch;
        private bool isResetSimulationAvailable;
        private bool simulateFirstTouch;

        private int charactersSavedAmount;
        private int activeCharactersAmount;

        private UISkinStore uiStore;

        private InputManager inputManager;

        private int CharactersAmount => charactersList.Count;

        public void Init()
        {
            instance = this;

            startPointsPool = PoolManager.GetPoolByName("StartPoint");
            charactersPool = PoolManager.GetPoolByName("Character");
            pathTrailsPool = PoolManager.GetPoolByName("PathTrail");
            movingObstaclePool = PoolManager.GetPoolByName("MovingObstacle");
            movingObstaclePathRendererPool = PoolManager.GetPoolByName("MovingObstaclePathRenderer");

            for (int i = 0; i < LevelsDatabase.Data.Items.Length; i++)
            {
                LevelItem currentItem = LevelsDatabase.Data.Items[i];

                if (!currentItem.Item.Equals(Item.StartPoint) && !currentItem.Item.Equals(Item.Character) && !currentItem.Item.Equals(Item.MovingObstacle))
                {
                    IPool tempPool = new Pool(currentItem.Prefab, currentItem.Item.ToString());
                    itemsPoolsDict.Add(currentItem.Item, tempPool);
                }
            }

            mainTrailBehaviourRef = pathTrailsPool.GetPooledObject().GetComponent<TrailBehaviour>();
            secondTrailBehaviourRef = pathTrailsPool.GetPooledObject().GetComponent<TrailBehaviour>();

            uiStore = UIController.GetPage<UISkinStore>();
            uiGame = UIController.GetPage<UIGame>();
            uiMainMenu = UIController.GetPage<UIMainMenu>();

            inputManager = GetComponent<InputManager>();

            inputManager.OnMouseClickDown += OnMouseClickDown;
            inputManager.OnMouseClickUp += OnMouseClickUp;

            SkinsController.SkinSelected += OnSkinSelected;
        }

        private bool mouseClicked = false;

        private void OnMouseClickDown(Vector2 obj)
        {
            if (!IsPlaying)
            {
                return;
            }
            mouseClicked = true;
            skipTouch = true;
            simulateFirstTouch = false;
            RaycastHit hit;
            Ray ray = cameraRef.ScreenPointToRay(InputManager.MousePosition);

            StartPointBehaviour startPoint = null;

            if (Physics.Raycast(ray, out hit, 1000f, startPointLayer))
            {
                startPoint = hit.collider.GetComponent<StartPointBehaviour>();
            }

            if (startPoint == null && Physics.Raycast(ray, out hit, 1000f, boatLayer) && boatBehaviour.transform.position == CurrentLevel.StartPoint)
            {
                startPoint = this.startPoint;
            }

            if (startPoint != null)
            {
                resetTrail = true;
                skipTouch = false;
                lastStartPointPosition = hit.collider.gameObject.transform.position;

                StopSimulation(true);

                if (UIController.GetPage<UIMainMenu>().IsPageDisplayed)
                {
                    UIController.HidePage<UIMainMenu>();
                }

                uiGame.HideTutorial();
            }
        }

        private void OnMouseClickUp(Vector2 obj)
        {
            if (!IsPlaying)
            {
                return;
            }
            mouseClicked = false;

            if (!skipTouch)
            {
                OnPathDrawn();
            }
        }


        private void OnDestroy()
        {
            if (itemsPoolsDict != null)
            {
                foreach (IPool item in itemsPoolsDict.Values)
                {
                    if (item != null)
                    {
                        PoolManager.DestroyPool(item);
                    }
                }
            }

            inputManager.OnMouseClickDown -= OnMouseClickDown;
            inputManager.OnMouseClickUp -= OnMouseClickUp;
        }

        public static void MultiplyReward(float multiplier)
        {
            CoinsPickedAmount = (int)(CoinsPickedAmount * multiplier);
        }

        public static void ApplyReward()
        {
            CurrenciesController.Add(CurrencyType.Coins, CoinsPickedAmount);
        }

        public void LoadLevel(Level level)
        {
            CurrentLevel = level;
            uiMainMenu.UpdateLevelText(GameController.CurrentLevelIndex + 1);

            skipTouch = true;
            isResetSimulationAvailable = false;
            charactersSavedAmount = 0;
            TotalCoinsPerLevel = 0;
            CoinsPickedAmount = 0;

            ReturnToPoolAllItems();
            interactablesList.Clear();

            startPointsPool.ReturnToPoolEverything();

            charactersPool.ReturnToPoolEverything();
            charactersList.Clear();

            movingObstaclePool.ReturnToPoolEverything();
            movingObstaclesList.Clear();


            CollectableBehavior.ResetCounter();

            // lifeguard init
            startPoint = startPointsPool.GetPooledObject().SetPosition(level.StartPoint).GetComponent<StartPointBehaviour>();
            startPoint.Init();

            if (boatBehaviour == null)
            {
                BoatSkinData boatData = (BoatSkinData)SkinsController.Instance.GetSelectedSkin<BoatSkinsDatabase>();

                boatBehaviour = Instantiate(boatData.Prefab, level.StartPoint, Quaternion.identity).GetComponent<BoatBehaviour>();
            }
            else
            {
                boatBehaviour.transform.position = level.StartPoint;
                boatBehaviour.transform.rotation = Quaternion.identity;
            }

            boatBehaviour.Init(startPoint, mainTrailBehaviourRef);

            mainTrailBehaviourRef.Init(true);
            secondTrailBehaviourRef.Init(false);

            // characters init
            for (int i = 0; i < level.CharactersList.Count; i++)
            {
                charactersList.Add(charactersPool.GetPooledObject().SetPosition(level.CharactersList[i].ItemSave.Position).GetComponent<CharacterBehaviour>());
                charactersList[i].Init(level.CharactersList[i]);
            }

            // moving obstacles init
            for (int i = 0; i < level.MovingObstaclesList.Count; i++)
            {
                movingObstaclesList.Add(movingObstaclePool.GetPooledObject().GetComponent<MovingObstacleBehaviour>());
                movingObstaclesList[i].Init(level.MovingObstaclesList[i]);

                var interactable = movingObstaclesList[i] as IInteractable;

                if (interactable != null)
                    interactablesList.Add(interactable);
            }

            // items init
            for (int i = 0; i < level.ItemsList.Count; i++)
            {
                ItemSave item = level.ItemsList[i];

                GameObject obj = itemsPoolsDict[item.Type].GetPooledObject();
                obj.transform.position = item.Position;
                obj.transform.eulerAngles = item.Rotation;
                obj.transform.localScale = item.Scale;

                CollectableBehavior initializable = obj.GetComponent<CollectableBehavior>();

                if (obj.CompareTag("Coin"))
                {
                    TotalCoinsPerLevel++;
                }

                if (initializable != null)
                    initializable.Init();

                var interactable = obj.GetComponent<IInteractable>();

                if (interactable != null)
                    interactablesList.Add(interactable);
            }

            isInputActive = false;
        }

        private void OnSkinSelected(ISkinData skinData)
        {
            if (skinData is BoatSkinData)
            {
                BoatSkinData boatSkinData = (BoatSkinData)skinData;

                if (boatBehaviour == null)
                    return;

                BoatBehaviour newBoatBehaviour = Instantiate(boatSkinData.Prefab, boatBehaviour.transform.position, boatBehaviour.transform.rotation).GetComponent<BoatBehaviour>();

                Destroy(boatBehaviour);

                boatBehaviour = newBoatBehaviour;
                boatBehaviour.Init(startPoint, mainTrailBehaviourRef);
            }
        }

        public void StartGame()
        {
            simulateFirstTouch = true;
            isInputActive = true;
            IsPlaying = true;
        }

        private void Update()
        {
            if (!isInputActive)
                return;

            if (uiStore.IsPageDisplayed)
                return;

            ///TODO: Before starting the game, set an UI Canvas behind the PLAY button
            if (simulateFirstTouch)
            {
                OnMouseClickDown(InputManager.MousePosition);
            }

            if (!skipTouch && mouseClicked)
            {
                RaycastHit hit;
                Ray ray = cameraRef.ScreenPointToRay(InputManager.MousePosition);

                if (Physics.Raycast(ray, out hit, 1000f, touchLayer))
                {
                    if (hit.collider.CompareTag("Ground"))
                    {
                        UpdatePath(hit.point);
                    }
                    else
                    {
                        UpdatePath(hit.point.SetY(0f));
                    }
                }
            }
        }

        private void UpdatePath(Vector3 newPosition)
        {
            if (Vector3.Distance(newPosition, lastTrailPoint) > minTrailPointsDistance)
            {
                mainTrailBehaviourRef.Transform.position = Vector3.Lerp(mainTrailBehaviourRef.Transform.position, newPosition, 0.5f);
                lastTrailPoint = newPosition;
            }

            if (resetTrail)
            {
                resetTrail = false;
                mainTrailBehaviourRef.Reset(lastStartPointPosition);
                secondTrailBehaviourRef.Reset(lastStartPointPosition);
            }
        }

        private void OnPathDrawn(bool setAdditionalPosition = false, Vector3 additionalPosition = new Vector3())
        {
            Vector3[] pointsArray = new Vector3[mainTrailBehaviourRef.TrailRenderer.positionCount + (setAdditionalPosition ? 1 : 0)];
            mainTrailBehaviourRef.TrailRenderer.GetPositions(pointsArray);

            if (setAdditionalPosition)
            {
                pointsArray[pointsArray.Length - 1] = additionalPosition;
            }

            List<Vector3> smoothedPointsList = new List<Vector3>();

            if (pointsArray.Length > 12)
            {
                Vector3[] smoothPathStart = MakeSmoothCurve(pointsArray.SubArray(0, 5), 1f);

                smoothedPointsList.AddRange(smoothPathStart);


                Vector3[] mid = pointsArray.SubArray(smoothPathStart.Length, pointsArray.Length - 10);
                smoothedPointsList.AddRange(mid);

                Vector3[] smoothPathEnding = MakeSmoothCurve(pointsArray.SubArray(pointsArray.Length - 5, 5), 1f);

                smoothedPointsList.AddRange(smoothPathEnding);

                int trailPointsAmount = mainTrailBehaviourRef.TrailRenderer.positionCount;
                int smoothedPathPointsAmount = smoothedPointsList.Count;

                for (int i = 1; i < 5; i++)
                {
                    mainTrailBehaviourRef.TrailRenderer.SetPosition(trailPointsAmount - i, smoothedPointsList[smoothedPathPointsAmount - (i + 1)]);
                }
            }
            else
            {
                smoothedPointsList.AddRange(pointsArray);
            }

            mainTrailBehaviourRef.PathPointsList = smoothedPointsList.ToArray();

            StartSimulation();
        }

        private Vector3[] MakeSmoothCurve(Vector3[] arrayToCurve, float smoothness)
        {
            List<Vector3> points;
            List<Vector3> curvedPoints;
            int pointsLength = 0;
            int curvedLength = 0;

            if (smoothness < 1.0f)
                smoothness = 1.0f;

            pointsLength = arrayToCurve.Length;

            curvedLength = pointsLength * Mathf.RoundToInt(smoothness) - 1;
            curvedPoints = new List<Vector3>(curvedLength);

            float t = 0.0f;
            for (int pointInTimeOnCurve = 0; pointInTimeOnCurve < curvedLength + 1; pointInTimeOnCurve++)
            {
                t = Mathf.InverseLerp(0, curvedLength, pointInTimeOnCurve);

                points = new List<Vector3>(arrayToCurve);

                for (int j = pointsLength - 1; j > 0; j--)
                {
                    for (int i = 0; i < j; i++)
                    {
                        points[i] = (1 - t) * points[i] + t * points[i + 1];
                    }
                }

                curvedPoints.Add(points[0]);
            }

            return curvedPoints.ToArray();
        }

        private void StartSimulation()
        {
            if (mainTrailBehaviourRef == null)
                return;

            if (!mainTrailBehaviourRef.PathAvailable)
                return;

            boatBehaviour.StarMovement(mainTrailBehaviourRef.PathPointsList);

            secondTrailBehaviourRef.CopyPointsFrom(mainTrailBehaviourRef);

            startPoint.PlayLightAnimation();
            isResetSimulationAvailable = true;
            activeCharactersAmount = charactersList.Count;

            for (int i = 0; i < charactersList.Count; i++)
            {
                charactersList[i].RunSimulation();
            }

            for (int i = 0; i < movingObstaclesList.Count; i++)
            {
                movingObstaclesList[i].StarMovement();
            }
        }

        private void StopSimulation(bool resetProgress)
        {
            isResetSimulationAvailable = false;

            if (resetProgress)
            {
                boatBehaviour.ResetPosition();
                startPoint.PlayIdleAnimation();

                charactersSavedAmount = 0;

                for (int i = 0; i < charactersList.Count; i++)
                {
                    charactersList[i].ResetCharacter();
                }

                for (int i = 0; i < movingObstaclesList.Count; i++)
                {
                    movingObstaclesList[i].ResetPosition();
                }

                for (int i = 0; i < interactablesList.Count; i++)
                {
                    interactablesList[i].Reinit();
                }
            }
            else
            {
                for (int i = 0; i < movingObstaclesList.Count; i++)
                {
                    movingObstaclesList[i].StopMovement();
                }
            }
        }

        public void OnCharacterSaved()
        {
            activeCharactersAmount--;
            charactersSavedAmount++;

            AudioController.PlaySound(AudioController.AudioClips.saved);

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            if (activeCharactersAmount <= 0 && !boatBehaviour.IsMovementActive)
            {
                CompleteTheLevel();
            }
        }

        public void OnCharacterDrowned()
        {
            if (!isInputActive)
                return;

            activeCharactersAmount--;

            AudioController.PlaySound(AudioController.AudioClips.drown, 0.7f);

#if MODULE_HAPTIC
            Haptic.Play(Haptic.HAPTIC_LIGHT);
#endif

            if (activeCharactersAmount <= 0 && !boatBehaviour.IsMovementActive)
            {
                CompleteTheLevel();
            }
        }

        public static void OnBoatDrowned()
        {
            instance.charactersSavedAmount = 0;
            instance.CompleteTheLevel();
        }

        public static void OnCurrencyPicked(CurrencyType type, int amount)
        {
            if (type == CurrencyType.Coins)
                CoinsPickedAmount += amount;
        }

        public void OnMovementCompleted()
        {
            if (activeCharactersAmount <= 0)
                CompleteTheLevel();
        }

        private void CompleteTheLevel()
        {
            if (!isInputActive)
                return;

            isInputActive = false;
            isResetSimulationAvailable = false;

            StopSimulation(false);
            boatBehaviour.StopMovement();

            if (charactersSavedAmount > 0)
            {
                int starsAmount = 1;

                if (charactersSavedAmount == CharactersAmount)
                {
                    starsAmount += 1;
                }

                if (CoinsPickedAmount >= TotalCoinsPerLevel)
                {
                    starsAmount += 1;
                }

                OnLevelComplete();
            }
            else
            {
                OnLevelFailed();
            }
        }

        public void Undo()
        {
            if (isResetSimulationAvailable)
            {
                StopSimulation(true);

                mainTrailBehaviourRef.Reset(startPoint.Position);
            }

            CoinsPickedAmount = 0;
        }

        private void OnLevelComplete()
        {
            Tween.DelayedCall(0.5f,
                () =>
                    {
                        AudioController.PlaySound(AudioController.AudioClips.complete);

                        GameController.Instance.LevelComplete();
                    });
        }

        public void OnLevelFailed()
        {
            Tween.DelayedCall(0.5f,
                () =>
                    {
                        AudioController.PlaySound(AudioController.AudioClips.failed);
                        GameController.Instance.LevelFailed();
                    });
        }

        public void OnLevelBeingSkiped()
        {
            StopSimulation(false);
        }

        public void ReturnToPoolAllItems()
        {
            foreach (KeyValuePair<Item, IPool> entry in itemsPoolsDict)
            {
                entry.Value.ReturnToPoolEverything();
            }

            movingObstaclePathRendererPool.ReturnToPoolEverything();
        }
    }
}
