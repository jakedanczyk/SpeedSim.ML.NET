using System;
using System.Collections.Generic;
using System.Text;

namespace SpeedSimML
{
    public static class Units
    {
        public static Dictionary<UnitType, Unit> UnitDict = new Dictionary<UnitType, Unit>()
        {
            {
                UnitType.SC,
                    new Ship{ Name = "Small Cargo",
                        MetalCost = 2000, CrysCost = 2000, DeutCost = 0,
                        FuelUsage = 20,
                        Speed = 10000,
                        Drive = DriveType.Combustion
                    }                        
            },

            {
				UnitType.LC,
                    new Ship{ Name = "Large Cargo",
                        MetalCost = 6000, CrysCost = 6000, DeutCost = 0,
                        FuelUsage = 50,
                        Speed = 7500,
                        Drive = DriveType.Combustion
                    }
            },

            {
				UnitType.LF,
                    new Ship{ Name = "Light Fighter",
                        MetalCost = 3000, CrysCost = 1000, DeutCost = 0,
                        FuelUsage = 20,
                        Speed = 12500,
                        Drive = DriveType.Combustion
                    }
            },

            {
				UnitType.HF,
                    new Ship{ Name = "Heavy Fighter",
                        MetalCost = 6000, CrysCost = 4000, DeutCost = 0,
                        FuelUsage = 75,
                        Speed = 10000,
                        Drive = DriveType.Impulse
                    }
                },

            {
				UnitType.C,
                    new Ship{ Name = "Cruiser",
                        MetalCost = 20000, CrysCost = 7000, DeutCost = 2000,
                        FuelUsage = 300,
                        Speed = 15000,
                        Drive = DriveType.Impulse
                    }
            },

            {
				UnitType.BS,
                    new Ship{ Name = "Battleship",
                        MetalCost = 45000, CrysCost = 15000, DeutCost = 0,
                        FuelUsage = 500,
                        Speed = 10000
                        Drive = DriveType.Hyperspace
                    }
            },

            {
				UnitType.Colony,
                    new Ship{ Name =  "Colony Ship",
                        MetalCost = 10000, CrysCost = 20000, DeutCost = 10000,
                        FuelUsage = 1000,
                        Speed = 2500,
                        Drive = DriveType.Impulse
                    }
            },

            {
				UnitType.Rec,
					new Ship{ Name = "Recycler",
                        MetalCost = 10000, CrysCost = 6000, DeutCost = 2000,
                        FuelUsage = 300,
                        Speed = 2000,
                        Drive = DriveType.Combustion
                    }
            },

            {
				UnitType.Probe,
					new Ship{ Name =  "Espionage Probe",
                        MetalCost = 0, CrysCost = 1000, DeutCost = 0,
                        FuelUsage = 1,
                        Speed = 100000000,
                        Drive = DriveType.Combustion
                    }
            },

            {
				UnitType.B,
					new Ship{ Name = "Bomber",
                        MetalCost = 50000, CrysCost = 25000, DeutCost = 15000,
                        FuelUsage = 1000,
                        Speed = 5000,
                        Drive = DriveType.Impulse
                    }
            },

            {
				UnitType.Sat,
					new Unit{ Name = "Solar Satellite",
                        MetalCost = 0, CrysCost = 2000, DeutCost = 500,
                    }
			},

            {
				UnitType.D,
					new Ship{ Name = "Destoyer",
                        MetalCost = 60000, CrysCost = 50000, DeutCost = 15000,
                        FuelUsage = 1000,
                        Speed = 5000,
                        Drive = DriveType.Hyperspace
                    }
			},

            {
				UnitType.DS,
					new Ship{ Name = "Deathstar",
                        MetalCost = 5000000, CrysCost = 4000000, DeutCost = 1000000,
                        FuelUsage = 1,
                        Speed = 100,
                        Drive = DriveType.Hyperspace
                    }
			},

            {
				UnitType.BC,
					new Ship{ Name = "Battlecruiser",
                        MetalCost = 30000, CrysCost = 40000, DeutCost = 15000,
                        FuelUsage = 250,
                        Speed = 10000,
                        Drive = DriveType.Hyperspace
                    }
			},

            {
				UnitType.RL,
					new Unit{ Name = "Rocket Launcher",
                        MetalCost = 2000, CrysCost = 0, DeutCost = 0 }
			},

            {
				UnitType.LL,
					new Unit{ Name = "Light Laser",
                        MetalCost = 1500, CrysCost = 500, DeutCost = 0 }
			},

            {
				UnitType.HL,
					new Unit{ Name = "Heavy Laser" ,
                        MetalCost = 6000, CrysCost = 2000, DeutCost = 0 }
			},

            {
				UnitType.GC,
					new Unit{ Name = "Gauss Cannon",
                        MetalCost = 20000, CrysCost = 15000, DeutCost = 2000 }
			},

            {
				UnitType.IC,
					new Unit{ Name = "Ion Cannon",
                        MetalCost = 2000, CrysCost = 6000, DeutCost = 0 }
			},

            {
				UnitType.PT,
					new Unit{ Name = "Plasma Turret",
                        MetalCost = 50000, CrysCost = 50000, DeutCost = 30000 }
			},

            {
				UnitType.SSD,
					new Unit{ Name = "Small Shield Dome",
                        MetalCost = 10000, CrysCost = 10000, DeutCost = 0 }
			},

            {
				UnitType.LSD,
					new Unit{ Name = "Large Shield Dome",
                        MetalCost = 50000, CrysCost = 50000, DeutCost = 0 }
			}
        };


        public static Unit GetUnit(UnitType unitType)
        {
            return UnitDict[unitType];
        }
    }
}
