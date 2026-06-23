using DiaToMas.Data;
using DiaToMas.Managers;
using UnityEngine;

namespace DiaToMas.Interaction
{
    public class WorldItemDropper : MonoBehaviour
    {
        [SerializeField] private WorldItemPickup _pickupPrefab;
        [SerializeField] private float _dropDistance = 1.8f;
        [SerializeField] private float _dropHeight = 0.35f;

        public bool TryDrop(ShopItemData itemData, int amount, out string message)
        {
            if (itemData == null || GameManager.Inst == null || _pickupPrefab == null)
            {
                message = "드롭 정보를 확인할 수 없습니다.";
                return false;
            }

            if (amount <= 0)
            {
                message = "드롭 수량이 올바르지 않습니다.";
                return false;
            }

            if (!GameManager.Inst.PlayerModel.InventoryModel.TryRemoveItem(itemData.id, amount))
            {
                message = $"{itemData.displayName} 보유 수량이 부족합니다.";
                return false;
            }

            WorldItemPickup pickup = Instantiate(_pickupPrefab, GetDropPosition(), Quaternion.identity);
            pickup.name = $"Dropped_{itemData.id}";
            pickup.Setup(itemData.id, amount);
            message = $"{itemData.displayName} {amount}개를 바닥에 내려놓았습니다.";
            return true;
        }

        private Vector3 GetDropPosition()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return transform.position + Vector3.forward * _dropDistance + Vector3.up * _dropHeight;
            }

            Vector3 forward = Vector3.ProjectOnPlane(player.transform.forward, Vector3.up);
            forward = forward.sqrMagnitude > 0.001f ? forward.normalized : Vector3.forward;
            return player.transform.position + forward * _dropDistance + Vector3.up * _dropHeight;
        }
    }
}
