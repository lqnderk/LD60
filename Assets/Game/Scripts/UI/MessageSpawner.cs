using System.Collections.Generic;
using UnityEngine;

namespace Game.UI
{
    public class MessageSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject messageZone;
        [SerializeField] private GameObject[] messagePrefabs;
        [SerializeField] private Target target;

        [Header("Timing")]
        [SerializeField] private float minInterval = 5f;
        [SerializeField] private float maxInterval = 10f;

        [Header("Layout")]
        [SerializeField] private int maxMessages = 5;
        [SerializeField] private float spacing = 0.15f;
        [Range(0f, 0.45f)] [SerializeField] private float padding = 0.05f;

        [Header("Reward")]
        [SerializeField] private int closeReward = 20;
        [SerializeField] private MoneyManager moneyManager;

        [Header("Sorting")]
        [SerializeField] private int messageSortingOrder = 25;

        [Header("Main Screen Spam")]
        [SerializeField] private GameObject spamZone;
        [SerializeField] private float fullTimeout = 10f;
        [SerializeField] private float spamInterval = 3f;
        [SerializeField] private int spamLimit = 15;
        [SerializeField] private Vector2 spamAreaSize = new Vector2(10f, 5f);
        [SerializeField] private int spamSortingOrder = 30;

        private readonly List<GameObject> messages = new List<GameObject>();
        private readonly List<GameObject> spamMessages = new List<GameObject>();
        private float timer;
        private float fullTimer;
        private float spamTimer;
        private bool spamming;
        private bool zoneWasActive;

        private void Awake()
        {
            if (target == null)
            {
                target = FindFirstObjectByType<Target>();
            }

            if (moneyManager == null)
            {
                moneyManager = FindFirstObjectByType<MoneyManager>();
            }
        }

        private void Start()
        {
            timer = Random.Range(minInterval, maxInterval);
        }

        private void Update()
        {
            UpdateSpam();

            if (messageZone == null || messagePrefabs == null || messagePrefabs.Length == 0 || maxMessages <= 0)
            {
                return;
            }

            if (messageZone.activeInHierarchy)
            {
                zoneWasActive = true;
            }

            if (!IsUnlocked)
            {
                timer = Random.Range(minInterval, maxInterval);
                return;
            }

            timer -= Time.deltaTime;

            if (timer > 0f)
            {
                return;
            }

            timer = Random.Range(minInterval, maxInterval);
            Spawn();
        }

        private bool IsUnlocked => zoneWasActive && target != null && target.FirstTargetReached;

        private void Spawn()
        {
            Cleanup();

            if (messages.Count >= maxMessages)
            {
                Destroy(messages[0]);
                messages.RemoveAt(0);
            }

            GameObject prefab = messagePrefabs[Random.Range(0, messagePrefabs.Length)];

            if (prefab == null)
            {
                return;
            }

            GameObject message = Instantiate(prefab, messageZone.transform);
            message.transform.localScale = KeepPrefabScale(prefab.transform.localScale, messageZone);
            SortingBand.ApplyTo(message, messageSortingOrder);
            InitializeButtons(message);
            messages.Add(message);
            AudioManager.Instance.PlayMessage(messageZone.activeInHierarchy);

            Layout();
        }

        public void Remove(GameObject message)
        {
            if (message == null)
            {
                return;
            }

            messages.Remove(message);
            Destroy(message);
            Layout();
            ClearSpam();

            if (moneyManager != null)
            {
                moneyManager.AddFlatMoney(closeReward);
            }
        }

        private void UpdateSpam()
        {
            if (spamZone == null || messagePrefabs == null || messagePrefabs.Length == 0 || spamLimit <= 0)
            {
                return;
            }

            if (spamming)
            {
                spamTimer += Time.deltaTime;

                if (spamTimer < spamInterval)
                {
                    return;
                }
            }
            else
            {
                spamTimer = 0f;

                if (messages.Count < maxMessages)
                {
                    fullTimer = 0f;
                    return;
                }

                fullTimer += Time.deltaTime;

                if (fullTimer < fullTimeout)
                {
                    return;
                }

                spamming = true;
            }

            spamTimer = 0f;
            SpawnSpam();
        }

        private void SpawnSpam()
        {
            GameObject prefab = messagePrefabs[Random.Range(0, messagePrefabs.Length)];

            if (prefab == null)
            {
                return;
            }

            Bounds area = GetArea(spamZone, spamAreaSize);

            GameObject spam = Instantiate(prefab, spamZone.transform);
            spam.transform.localScale = KeepPrefabScale(prefab.transform.localScale, spamZone);
            spam.transform.position = new Vector3(
                Random.Range(area.min.x, area.max.x),
                Random.Range(area.min.y, area.max.y),
                area.center.z);
            spam.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-5f, 5f));

            SortingBand.ApplyTo(spam, spamSortingOrder);
            NeutralizeButtons(spam);

            spamMessages.Add(spam);

            if (spamMessages.Count > spamLimit)
            {
                GameObject oldest = spamMessages[0];
                spamMessages.RemoveAt(0);
                Destroy(oldest);
            }

            AudioManager.Instance.PlayMessage(false);
        }

        private void ClearSpam()
        {
            for (int i = 0; i < spamMessages.Count; i++)
            {
                if (spamMessages[i] != null)
                {
                    Destroy(spamMessages[i]);
                }
            }

            spamMessages.Clear();
            spamming = false;
            fullTimer = 0f;
            spamTimer = 0f;
        }

        private void NeutralizeButtons(GameObject spam)
        {
            var buttons = spam.GetComponentsInChildren<MessageCloseButton>(true);

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Initialize(null, this);
            }
        }

        private void InitializeButtons(GameObject message)
        {
            var buttons = message.GetComponentsInChildren<MessageCloseButton>(true);

            for (int i = 0; i < buttons.Length; i++)
            {
                buttons[i].Initialize(message, this);
            }
        }

        private Vector3 KeepPrefabScale(Vector3 scale, GameObject zone)
        {
            Vector3 zoneScale = zone.transform.lossyScale;

            return new Vector3(
                zoneScale.x != 0f ? scale.x / zoneScale.x : scale.x,
                zoneScale.y != 0f ? scale.y / zoneScale.y : scale.y,
                zoneScale.z != 0f ? scale.z / zoneScale.z : scale.z);
        }

        private void Cleanup()
        {
            for (int i = messages.Count - 1; i >= 0; i--)
            {
                if (messages[i] == null)
                {
                    messages.RemoveAt(i);
                }
            }
        }

        private void Layout()
        {
            Bounds area = GetArea(messageZone, Vector3.one);
            float centerX = area.center.x;
            float top = area.max.y - area.size.y * padding;

            for (int i = 0; i < messages.Count; i++)
            {
                GameObject message = messages[i];

                if (message == null)
                {
                    continue;
                }

                float height = GetHeight(message);

                message.transform.position = new Vector3(centerX, top - height * 0.5f, area.center.z);
                top -= height + spacing;
            }
        }

        private Bounds GetArea(GameObject zone, Vector3 fallbackSize)
        {
            var rect = zone.GetComponent<RectTransform>();

            if (rect != null)
            {
                var corners = new Vector3[4];
                rect.GetWorldCorners(corners);

                var bounds = new Bounds(corners[0], Vector3.zero);

                for (int i = 1; i < corners.Length; i++)
                {
                    bounds.Encapsulate(corners[i]);
                }

                return bounds;
            }

            var sprite = zone.GetComponent<SpriteRenderer>();

            if (sprite != null)
            {
                return sprite.bounds;
            }

            return new Bounds(zone.transform.position, fallbackSize);
        }

        private float GetHeight(GameObject message)
        {
            var rect = message.GetComponent<RectTransform>();

            if (rect != null)
            {
                return rect.rect.height * Mathf.Abs(message.transform.lossyScale.y);
            }

            var sprite = message.GetComponent<SpriteRenderer>();

            if (sprite != null)
            {
                return sprite.bounds.size.y;
            }

            return 1f;
        }
    }
}
