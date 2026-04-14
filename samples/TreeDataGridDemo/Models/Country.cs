namespace TreeDataGridDemo.Models;

internal class Country(string name, string region, int population, int area, double density, double coast, double? migration,
               double? infantMorality, int gdp, double? literacy, double? phones, double? birth, double? death)
{
    public string? Name { get; set; } = name;
    public string Region { get; private set; } = region;
    public int Population { get; private set; } = population;
    //Square Miles
    public int Area { get; private set; } = area;
    //Per Square Mile
    public double PopulationDensity { get; private set; } = density;
    //Coast / Area
    public double CoastLine { get; private set; } = coast;
    public double? NetMigration { get; private set; } = migration;
    //per 1000 births
    public double? InfantMortality { get; private set; } = infantMorality;
    public int GDP { get; private set; } = gdp;
    public double? LiteracyPercent { get; private set; } = literacy;
    //per 1000
    public double? Phones { get; private set; } = phones;
    public double? BirthRate { get; private set; } = birth;
    public double? DeathRate { get; private set; } = death;
}
