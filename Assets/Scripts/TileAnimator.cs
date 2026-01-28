using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileAnimator : MonoBehaviour
{
    [SerializeField] private float swapDuration = 0.25f;
    [SerializeField] private float deletePopDuration = 0.10f;
    [SerializeField] private float deleteShrinkDuration = 0.15f;
    [SerializeField] private float popMultiplier = 1.20f;

    private bool isAnimating;

    public void PlaySwap(Tile firstTile, Vector3 firstTarget, Tile secondTile, Vector3 secondTarget)
    {
        PlaySwap(firstTile, firstTarget, secondTile, secondTarget, null);

    }
    public void PlaySwap(Tile firstTile, Vector3 firstTarget, Tile secondTile, Vector3 secondTarget, System.Action onComplete)
    {

        if(isAnimating) return;

        StartCoroutine(SwapCoroutine(firstTile, firstTarget, secondTile, secondTarget, onComplete));
    }

    public void PlayDelete(List<Tile> tiles, System.Action onComplete)
    {
        if(isAnimating) return;
        if(tiles == null || tiles.Count == 0)
        {
            onComplete?.Invoke();
            return;
        } 

        StartCoroutine(DeleteCoroutine(tiles, onComplete));
    }

    private IEnumerator DeleteCoroutine(List<Tile> tiles, System.Action onComplete)
    {
        isAnimating = true;

        List<Tile> aliveTiles = new List<Tile>();
        Dictionary<Tile, Vector3> startScales = new Dictionary<Tile, Vector3>();

        for(int i = 0; i < tiles.Count; i++)
        {
            Tile tile = tiles[i];
            if(tile == null) continue;
            aliveTiles.Add(tile);
            startScales[tile] = tile.transform.localScale;
        }

        if(aliveTiles.Count == 0)
        {
            isAnimating = false;
            onComplete?.Invoke();
            yield break;
        }

        float elapsed = 0f;
        while(elapsed < deletePopDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / deletePopDuration;

            for(int i = 0; i < aliveTiles.Count; i++)
            {
                Tile tile = aliveTiles[i];
                if(tile == null) continue;
                Vector3 start = startScales[tile];
                Vector3 pop = start * popMultiplier;
                tile.transform.localScale = Vector3.Lerp(start, pop, t);
            }
            
            yield return null;
        }

        for(int i = 0; i < aliveTiles.Count; i++)
        {
            Tile tile = aliveTiles[i];
            if(tile == null) continue;
            Vector3 start = startScales[tile];
            tile.transform.localScale = start * popMultiplier;
        }

        elapsed = 0f;
        while(elapsed < deleteShrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed/deleteShrinkDuration;
            
            for(int i = 0; i < aliveTiles.Count; i++)
            {
                Tile tile = aliveTiles[i];
                if(tile == null) continue;
                Vector3 start = startScales[tile];
                Vector3 end = Vector3.zero;
                tile.transform.localScale = Vector3.Lerp(start, end, t);
            }
            yield return null;
        }

        for(int i = 0; i < aliveTiles.Count; i++)
        {
            Tile tile = aliveTiles[i];
            if(tile == null) continue;
            tile.transform.localScale = Vector3.zero;
        }
        
        isAnimating = false;
        onComplete?.Invoke();
    }

    private IEnumerator SwapCoroutine(Tile firstTile, Vector3 firstTarget, Tile secondTile, Vector3 secondTarget, System.Action onComplete)
    {
        isAnimating = true;

        Vector3 firstStart = firstTile.transform.position;
        Vector3 secondStart = secondTile.transform.position;

        float elapsed = 0f;

        while(elapsed < swapDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / swapDuration;

            firstTile.transform.position = Vector3.Lerp(firstStart, firstTarget, t);
            secondTile.transform.position = Vector3.Lerp(secondStart, secondTarget, t);

            yield return null;
        }

        firstTile.transform.position = firstTarget;
        secondTile.transform.position = secondTarget;

        isAnimating = false;
        onComplete?.Invoke();
    }
}
