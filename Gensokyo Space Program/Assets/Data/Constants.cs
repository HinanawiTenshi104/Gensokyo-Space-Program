public static class Constants
{
    //metric unit
    public const double G = 6.6740831e-11;
    public const double AU = 149597870700.0;
    public const double KM = 1000.0;

    //my unit
    public const double TShrinkFactor = 10.0 / 86400.0;  //10�����һ�� 
    public const double OrbitSizeShrinkFactor = 2000.0 / AU;  //1AU����100��λ����
    public const double GFactor = 100.0*OrbitSizeShrinkFactor*OrbitSizeShrinkFactor;    //T=K/a^3
    public const double PlanetRadiusShrinkFactor = 1.0/(100.0*KM); //1��λ���ȵ���100km
}