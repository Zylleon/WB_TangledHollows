using RimWorld;
using RimWorld.Planet;

namespace ZWB_TangledHollows
{
    public class BiomeWorker_TangledHollows : BiomeWorker
    {

        public override float GetScore(BiomeDef biome, Tile tile, PlanetTile planetTile)
        {
            if (tile.WaterCovered)
            {
                return -100f;
            }
            if (tile.rainfall < 1000f || tile.rainfall > 2600f)
            {
                return 0;
            }
            if (tile.temperature < -10f || tile.temperature > 3.5f)
            {
                return 0;
            }
            //if (tile.swampiness > 0.5f)
            //{
            //    return 0f;
            //}
            //if (tile.elevation < 300f)
            //{
            //    return 0f;
            //}

            return 20f;
            //return 16.1f + tile.temperature;
        }
    }


}