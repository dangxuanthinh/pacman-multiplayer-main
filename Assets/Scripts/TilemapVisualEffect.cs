using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapVisualEffect : MonoBehaviour
{
    public Tilemap levelTilemap;
    private bool flashTilemap;
    private float alpha;

    private void Start()
    {
        StartAlphaAnimation();
    }

    public void StartAlphaAnimation()
    {
        Sequence alphaSequence = DOTween.Sequence();
        alphaSequence.Append(
            DOTween.To(() => alpha, x => alpha = x, 0.2f, 0.1f)
        );
        alphaSequence.Append(
            DOTween.To(() => alpha, x => alpha = x, 1f, 0.1f)
        );
        alphaSequence.SetLoops(-1);
    }

    public void SetTilemap(Tilemap tilemap)
    {
        this.levelTilemap = tilemap;
    }

    public void SetTilemapFlashing(bool flashTilemap)
    {
        this.flashTilemap = flashTilemap;
    }

    public void AnimateTileMapColor()
    {
        DOTween.Kill(gameObject);
        int colorCount = 20;
        float totalDuration = 3f;
        Sequence colorSequence = DOTween.Sequence();

        float durationPerColor = totalDuration / colorCount;

        for (int i = 0; i < colorCount; i++)
        {
            Color randomColor = new Color(Random.value, Random.value, Random.value);

            Color startColor = levelTilemap.color;

            colorSequence.Append(DOTween.To(
                () => levelTilemap.color,
                x => levelTilemap.color = x,
                randomColor,
                durationPerColor
            )).SetUpdate(UpdateType.Normal, true);
        }
    }

    private void Update()
    {
        if (levelTilemap == null) return;
        Color color = levelTilemap.color;
        if (flashTilemap == true)
            color.a = alpha;
        else
            color.a = 1;
        levelTilemap.color = color;
    }
}
