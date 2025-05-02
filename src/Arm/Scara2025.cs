using AutoExchange.RedemptionDetector;
using RapidlyArmPlanner.ArmSolver.ForwardDynamic;
using RapidlyArmPlanner.ArmSolver.InverseDynamic;
using RapidlyArmPlanner.ColliderDetector;
using RapidlyArmPlanner.PathFinder.RRT_BHAStar;

namespace Engineer.Arm;

class Scara2025 : Component
{
  RapidlyArmPlanner.ArmPlanner planner;

  // [ComponentReferenceFiled] IRedemptionDetector redemptionDetector;
  public override void Start()
  {
    var forwardDynamic = new Scara2025ForwardDynamic();
    var inverseDynamicSolver = new Scara2025InverseDynamic(
      0.3,
      0.3,
      0.05,
      (-Math.PI, Math.PI),
      (-Math.PI, Math.PI),
      (-120 * Math.PI / 180, 120 * Math.PI / 180)
    );
    var colliderDetector = new Scara2025BepuDetector();
    var searcher = new PathToPathLoose(
      new RRT_BHAStar(
        [Math.PI, 0.8, Math.PI, Math.PI, Math.PI, Math.PI],
        [-Math.PI, 0, -Math.PI, -Math.PI, -Math.PI, -Math.PI]
      )
      {
        forwardDynamic = forwardDynamic,
        obstacleDetector = colliderDetector,
      }
    );

    planner = new()
    {
      forwardDynamic = forwardDynamic,
      inverseDynamicSolver = inverseDynamicSolver,
      colliderDetector = colliderDetector,
      searcher = searcher,
    };
  }

  public override void Update() { }
}
