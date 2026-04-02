using UnityEngine;

namespace Eduzo.Games.BridgeConstruction
{
    public class BridgeConstructionDropSlot : MonoBehaviour
    {
        public BridgeConstructionGameManager GameManager;

        public void OnDropTile(BridgeConstructionOptionTile tile)
        {
            GameManager.OnTileDropped(tile);
        }
    }
}