using System;
using System.ComponentModel.DataAnnotations;

namespace Project420.Cultivation.Models.Enums
{
    /// <summary>
    /// Represents different types of growing environments
    /// </summary>
    /// <remarks>
    /// Different room types have different environmental requirements:
    /// - Mother Room: 18/6 light cycle, stable temps
    /// - Clone/Seedling: High humidity (70-80%), gentle light
    /// - Vegetative: 18/6 light cycle, moderate humidity (50-70%)
    /// - Flowering: 12/12 light cycle, lower humidity (40-50%)
    /// - Drying: Low humidity (50-60%), no light, air circulation
    /// </remarks>
    public enum GrowRoomType
    {
        /// <summary>
        /// Room for mother plants (genetic preservation)
        /// </summary>
        [Display(Name = "Mother Room")]
        MotherRoom = 0,

        /// <summary>
        /// Room for clones and seedlings
        /// </summary>
        [Display(Name = "Clone/Seedling Room")]
        CloneSeedlingRoom = 1,

        /// <summary>
        /// Vegetative growth room (18/6 light cycle)
        /// </summary>
        [Display(Name = "Vegetative Room")]
        VegetativeRoom = 2,

        /// <summary>
        /// Flowering room (12/12 light cycle)
        /// </summary>
        [Display(Name = "Flowering Room")]
        FloweringRoom = 3,

        /// <summary>
        /// Drying room for post-harvest
        /// </summary>
        [Display(Name = "Drying Room")]
        DryingRoom = 4
    }
}
