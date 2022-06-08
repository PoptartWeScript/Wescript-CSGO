using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.Mathematics;
using SharpDX.XInput;
using WeScriptWrapper;
using WeScript.SDK.UI;
using WeScript.SDK.UI.Components;

namespace CSGO
{
    class Program
    {
        public static float M_PI_F = (180.0f / Convert.ToSingle(System.Math.PI));

        public static bool gameProcessExists = false;
        public static bool isWow64Process = false;
        public static bool isGameOnTop = false;
        public static bool isOverlayOnTop = false;

        public static uint PROCESS_ALL_ACCESS = 0x1FFFFF;

        public static Vector2 wndMargins = new Vector2(0, 0);
        public static Vector2 wndSize = new Vector2(0, 0);
        public static Vector2 GameCenterPos = new Vector2(0, 0);
        public static Vector2 AimTarg2D = new Vector2(0, 0);
        public static Vector3 AimTarg3D = new Vector3(0, 0, 0);

        public static IntPtr processHandle = IntPtr.Zero;
        public static IntPtr client_panorama = IntPtr.Zero;
        public static IntPtr client_panorama_size = IntPtr.Zero;
        public static IntPtr engine_dll = IntPtr.Zero;
        public static IntPtr engine_dll_size = IntPtr.Zero;
        public static IntPtr dwViewMatrix_Offs = IntPtr.Zero;
        public static IntPtr dwEntityList_Offs = IntPtr.Zero;
        public static IntPtr dwLocalPlayer_Offs = IntPtr.Zero;
        public static IntPtr dwSetViewAng_Addr = IntPtr.Zero;
        public static IntPtr dwSetViewAng_Offs = IntPtr.Zero;

        public static int myHPBefore = 0;

        public static Menu RootMenu { get; private set; }
        public static Menu VisualsMenu { get; private set; }
        public static Menu AimbotMenu { get; private set; }
        public static Menu MiscMenu { get; private set; }

        class Components
        {
            public static readonly MenuKeyBind MainAssemblyToggle = new MenuKeyBind("mainassemblytoggle", "Toggle the whole assembly effect by pressing key:", VirtualKeyCode.Delete, KeybindType.Toggle, true);
            public static class VisualsComponent
            {
                public static readonly MenuBool DrawTheVisuals = new MenuBool("drawthevisuals", "Enable all of the Visuals", true);
                public static readonly MenuColor CTColor = new MenuColor("ctcolor", "CT ESP Color", new SharpDX.Color(0, 0, 255));
                public static readonly MenuBool DrawAlliesEsp = new MenuBool("drawalbox", "Draw Allies ESP", true);
                public static readonly MenuColor TRColor = new MenuColor("tercolor", "Terrorist ESP Color", new SharpDX.Color(255, 0, 0));
                public static readonly MenuBool DrawBox = new MenuBool("drawbox", "Draw Box ESP", true);
                public static readonly MenuSlider DrawBoxThic = new MenuSlider("boxthickness", "Draw Box Thickness", 0, 0, 10);
                public static readonly MenuBool DrawBoxBorder = new MenuBool("drawboxborder", "Draw Border around Box and Text?", true);
                public static readonly MenuBool DrawBoxHP = new MenuBool("drawboxhp", "Draw Health", true);
                public static readonly MenuSliderBool DrawTextSize = new MenuSliderBool("drawtextsize", "Text Size", false, 14, 4, 72);
                public static readonly MenuBool DrawTextDist = new MenuBool("drawtextdist", "Draw Distance", true);
            }
            public static class AimbotComponent
            {
                public static readonly MenuBool AimGlobalBool = new MenuBool("enableaim", "Enable Aimbot Features", true);
                public static readonly MenuKeyBind AimKey = new MenuKeyBind("aimkey", "Aimbot HotKey (HOLD)", VirtualKeyCode.LeftMouse, KeybindType.Hold, false);
                public static readonly MenuList AimType = new MenuList("aimtype", "Aimbot Type", new List<string>() { "Real Mouse Movement" }, 0);
                public static readonly MenuList AimSpot = new MenuList("aimspot", "Aimbot Spot", new List<string>() { "Aim at their Head", "Aim at their Body" }, 0);
                public static readonly MenuBool AIMRC = new MenuBool("aimrecoilcompens", "Compensate weapon recoil while aimbotting?", true);
                public static readonly MenuBool DrawRecoil = new MenuBool("drawrecoilpattern", "Draw Weapon Recoil Crosshair", true);
                public static readonly MenuColor RecoilColor = new MenuColor("recoilcolor", "Recoil Compensation Color", new SharpDX.Color(255, 0, 0, 255));
                public static readonly MenuBool AimAtEveryone = new MenuBool("aimeveryone", "Aim At Everyone (even teammates)", false);
                public static readonly MenuSlider AimSpeed = new MenuSlider("aimspeed", "Aimbot Speed %", 12, 1, 100);
                public static readonly MenuBool DrawAimSpot = new MenuBool("drawaimspot", "Draw Aimbot Spot", true);
                public static readonly MenuBool DrawAimTarget = new MenuBool("drawaimtarget", "Draw Aimbot Current Target", true);
                public static readonly MenuColor AimTargetColor = new MenuColor("aimtargetcolor", "Target Color", new SharpDX.Color(0x1F, 0xBE, 0xD6, 255));
                public static readonly MenuBool DrawAimFov = new MenuBool("drawaimfov", "Draw Aimbot FOV Circle", true);
                public static readonly MenuColor AimFovColor = new MenuColor("aimfovcolor", "FOV Color", new SharpDX.Color(255, 255, 255, 30));
                public static readonly MenuSlider AimFov = new MenuSlider("aimfov", "Aimbot FOV", 100, 4, 1000);
            }

        }

        public static void InitializeMenu()
        {
            VisualsMenu = new Menu("visualsmenu", "Visuals Menu")
            {
                Components.VisualsComponent.DrawTheVisuals,
                Components.VisualsComponent.CTColor,
                Components.VisualsComponent.DrawAlliesEsp.SetToolTip("Really great feature to increase performance by the way!"),
                Components.VisualsComponent.TRColor,
                Components.VisualsComponent.DrawBox,
                Components.VisualsComponent.DrawBoxThic.SetToolTip("Setting thickness to 0 will let the assembly auto-adjust itself depending on model distance"),
                Components.VisualsComponent.DrawBoxBorder.SetToolTip("Drawing borders may take extra performance (FPS) on low-end computers"),
                Components.VisualsComponent.DrawBoxHP,
                Components.VisualsComponent.DrawTextSize,
                Components.VisualsComponent.DrawTextDist,
            };

            AimbotMenu = new Menu("aimbotmenu", "Aimbot Menu")
            {
                Components.AimbotComponent.AimGlobalBool,
                Components.AimbotComponent.AimKey,
                Components.AimbotComponent.AimType,
                Components.AimbotComponent.AimSpot,
                Components.AimbotComponent.AIMRC,
                Components.AimbotComponent.DrawRecoil,
                Components.AimbotComponent.RecoilColor,
                Components.AimbotComponent.AimAtEveryone,
                Components.AimbotComponent.AimSpeed,
                Components.AimbotComponent.DrawAimSpot,
                Components.AimbotComponent.DrawAimTarget,
                Components.AimbotComponent.DrawAimFov,
                Components.AimbotComponent.AimFovColor,
                Components.AimbotComponent.AimFov,
            };

            RootMenu = new Menu("csgoExtension", "WeScript.app CSGO Push By Poptart", true)
            {
                Components.MainAssemblyToggle.SetToolTip("The magical boolean which completely disables/enables the assembly!"),
                VisualsMenu,
                AimbotMenu,
                MiscMenu,
            };
            RootMenu.Attach();
        }
        private static double GetDistance3D(Vector3 myPos, Vector3 enemyPos)
        {
            Vector3 vector = new Vector3(myPos.X - enemyPos.X, myPos.Y - enemyPos.Y, myPos.Z - enemyPos.Z);
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
        }

        private static double GetDistance2D(Vector2 pos1, Vector2 pos2)
        {
            Vector2 vector = new Vector2(pos1.X - pos2.X, pos1.Y - pos2.Y);
            return Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y);
        }

        private static Vector3 ReadBonePos(IntPtr playerPtr, int boneIDX)
        {
            Vector3 targetVec = new Vector3(0, 0, 0);
            var BoneMatrixPtr = Memory.ReadPointer(processHandle, (IntPtr)(playerPtr.ToInt64() + Offsets.m_dwBoneMatrix), isWow64Process);
            if (BoneMatrixPtr != IntPtr.Zero)
            {
                targetVec.X = Memory.ReadFloat(processHandle, (IntPtr)(BoneMatrixPtr.ToInt64() + 0x30 * boneIDX + 0x0C));
                targetVec.Y = Memory.ReadFloat(processHandle, (IntPtr)(BoneMatrixPtr.ToInt64() + 0x30 * boneIDX + 0x1C));
                targetVec.Z = Memory.ReadFloat(processHandle, (IntPtr)(BoneMatrixPtr.ToInt64() + 0x30 * boneIDX + 0x2C));
            }
            return targetVec;
        }

        public static Vector3 ClampAngle(Vector3 angle)
        {
            while (angle.Y > 180) angle.Y -= 360;
            while (angle.Y < -180) angle.Y += 360;

            if (angle.X > 89.0f) angle.X = 89.0f;
            if (angle.X < -89.0f) angle.X = -89.0f;

            angle.Z = 0f;

            return angle;
        }

        public static Vector3 NormalizeAngle(Vector3 angle)
        {
            while (angle.X < -180.0f) angle.X += 360.0f;
            while (angle.X > 180.0f) angle.X -= 360.0f;

            while (angle.Y < -180.0f) angle.Y += 360.0f;
            while (angle.Y > 180.0f) angle.Y -= 360.0f;

            while (angle.Z < -180.0f) angle.Z += 360.0f;
            while (angle.Z > 180.0f) angle.Z -= 360.0f;

            return angle;
        }

        public static Vector3 CalcAngle(Vector3 playerPosition, Vector3 enemyPosition, Vector3 aimPunch, Vector3 vecView, float yawRecoilReductionFactory, float pitchRecoilReductionFactor)
        {
            Vector3 delta = new Vector3(playerPosition.X - enemyPosition.X, playerPosition.Y - enemyPosition.Y, (playerPosition.Z + vecView.Z) - enemyPosition.Z);

            Vector3 tmp = Vector3.Zero;
            tmp.X = Convert.ToSingle(System.Math.Atan(delta.Z / System.Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y))) * 57.295779513082f - aimPunch.X * yawRecoilReductionFactory;
            tmp.Y = Convert.ToSingle(System.Math.Atan(delta.Y / delta.X)) * M_PI_F - aimPunch.Y * pitchRecoilReductionFactor;
            tmp.Z = 0;

            if (delta.X >= 0.0) tmp.Y += 180f;

            tmp = NormalizeAngle(tmp);
            tmp = ClampAngle(tmp);

            return tmp;
        }
        static void Main(string[] args)
        {
            Console.WriteLine("WeScript.app CSGO Example Assembly Loaded! (last update [15.01.2022])");

            InitializeMenu();
            Renderer.OnRenderer += OnRenderer;
            Memory.OnTick += OnTick;
        }
        private static void OnTick(int counter, EventArgs args)
        {
            if (processHandle == IntPtr.Zero)
            {
                var wndHnd = Memory.FindWindowClassName("Valve001");
                if (wndHnd != IntPtr.Zero)
                {
                    var calcPid = Memory.GetPIDFromHWND(wndHnd);
                    if (calcPid > 0)
                    {
                        processHandle = Memory.OpenProcess(PROCESS_ALL_ACCESS, calcPid);
                        if (processHandle != IntPtr.Zero)
                        {
                            isWow64Process = Memory.IsProcess64Bit(processHandle);
                        }
                    }
                }
            }
            else
            {
                var wndHnd = Memory.FindWindowClassName("Valve001");
                if (wndHnd != IntPtr.Zero)
                {
                    gameProcessExists = true;
                    wndMargins = Renderer.GetWindowMargins(wndHnd);
                    wndSize = Renderer.GetWindowSize(wndHnd);
                    isGameOnTop = Renderer.IsGameOnTop(wndHnd);
                    GameCenterPos = new Vector2(wndSize.X / 2 + wndMargins.X, wndSize.Y / 2 + wndMargins.Y);
                    isOverlayOnTop = Overlay.IsOnTop();

                    if (client_panorama == IntPtr.Zero)
                    {
                        client_panorama = Memory.GetModule(processHandle, "client.dll", isWow64Process);
                    }
                    else
                    {
                        if (client_panorama_size == IntPtr.Zero)
                        {
                            client_panorama_size = Memory.GetModuleSize(processHandle, "client.dll", isWow64Process);
                        }
                        else
                        {
                            if (dwViewMatrix_Offs == IntPtr.Zero)
                            {
                                dwViewMatrix_Offs = Memory.ReadPointer(processHandle, client_panorama + signatures.dwViewMatrix, isWow64Process);
                                //if (dwViewMatrix_Offs != IntPtr.Zero) Console.WriteLine($"dwViewMatrix_Offs: {dwViewMatrix_Offs.ToString("X")}");
                            }
                            if (dwEntityList_Offs == IntPtr.Zero)
                            {
                                dwEntityList_Offs = Memory.ReadPointer(processHandle, client_panorama + signatures.dwEntityList, isWow64Process);
                                //if (dwEntityList_Offs != IntPtr.Zero) Console.WriteLine($"dwEntityList_Offs: {dwEntityList_Offs.ToString("X")}");
                            }
                            if (dwLocalPlayer_Offs == IntPtr.Zero)
                            {
                                dwLocalPlayer_Offs = Memory.ReadPointer(processHandle, client_panorama + signatures.dwLocalPlayer, isWow64Process);
                                //if (dwLocalPlayer_Offs != IntPtr.Zero) Console.WriteLine($"dwLocalPlayer_Offs: {dwLocalPlayer_Offs.ToString("X")}");
                            }
                        }
                    }
                    if (engine_dll == IntPtr.Zero)
                    {
                        engine_dll = Memory.GetModule(processHandle, "engine.dll", isWow64Process);
                    }
                    else
                    {
                        if (engine_dll_size == IntPtr.Zero)
                        {
                            engine_dll_size = Memory.GetModuleSize(processHandle, "engine.dll", isWow64Process);
                        }
                    }
                }
                else
                {
                    Memory.CloseHandle(processHandle);
                    processHandle = IntPtr.Zero;
                    gameProcessExists = false;

                    //clear your offsets, modules
                    client_panorama = IntPtr.Zero;
                    engine_dll = IntPtr.Zero;
                    client_panorama_size = IntPtr.Zero;
                    engine_dll_size = IntPtr.Zero;
                    dwViewMatrix_Offs = IntPtr.Zero;
                    dwEntityList_Offs = IntPtr.Zero;
                    dwLocalPlayer_Offs = IntPtr.Zero;
                    dwSetViewAng_Addr = IntPtr.Zero;
                    dwSetViewAng_Offs = IntPtr.Zero;
                }
            }
        }

        private static void OnRenderer(int fps, EventArgs args)
        {
            if (!gameProcessExists) return;
            if ((!isGameOnTop) && (!isOverlayOnTop)) return;
            if (!Components.MainAssemblyToggle.Enabled) return;

            double fClosestPos = 999999;
            AimTarg2D = new Vector2(0, 0);
            AimTarg3D = new Vector3(0, 0, 0);

            if (dwViewMatrix_Offs != IntPtr.Zero)
            {
                //Console.WriteLine("wehere");
                var matrix = Memory.ReadMatrix(processHandle, (IntPtr)(dwViewMatrix_Offs.ToInt64() + 0xB0));
                if (dwEntityList_Offs != IntPtr.Zero)
                {
                    if (dwLocalPlayer_Offs != IntPtr.Zero)
                    {
                        var LocalPlayer = Memory.ReadPointer(processHandle, (IntPtr)(dwLocalPlayer_Offs.ToInt64() + 4), isWow64Process);
                        if (LocalPlayer != IntPtr.Zero)
                        {
                            var myPos = Memory.ReadVector3(processHandle, (IntPtr)(LocalPlayer.ToInt64() + 0x138));
                            var myTeam = Memory.ReadByte(processHandle, (IntPtr)(LocalPlayer.ToInt64() + Offsets.m_iTeamNum));
                            var myAngles = Memory.ReadVector3(processHandle, (IntPtr)(LocalPlayer.ToInt64() + Offsets.m_thirdPersonViewAngles));
                            var myEyePos = Memory.ReadVector3(processHandle, (IntPtr)(LocalPlayer.ToInt64() + Offsets.m_vecViewOffset));
                            var myPunchAngles = Memory.ReadVector3(processHandle, (IntPtr)(LocalPlayer.ToInt64() + Offsets.m_aimPunchAngle));
                            for (uint i = 0; i <= 64; i++)
                            {
                                var entityAddr = Memory.ReadPointer(processHandle, (IntPtr)(dwEntityList_Offs.ToInt64() + i * 0x10), isWow64Process);
                                if ((entityAddr != IntPtr.Zero) && (entityAddr != LocalPlayer))
                                {
                                    var m_iHealth = Memory.ReadInt32(processHandle, (IntPtr)(entityAddr.ToInt64() + Offsets.m_iHealth));
                                    var bDormant = Memory.ReadBool(processHandle, (IntPtr)(entityAddr.ToInt64() + signatures.m_bDormant));
                                    var m_iTeamNum = Memory.ReadByte(processHandle, (IntPtr)(entityAddr.ToInt64() + Offsets.m_iTeamNum));
                                    var m_vecOrigin = Memory.ReadVector3(processHandle, (IntPtr)(entityAddr.ToInt64() + Offsets.m_vecOrigin));
                                    var f_modelHeight = Memory.ReadFloat(processHandle, (IntPtr)(entityAddr.ToInt64() + 0x33C));
                                    var isenemybool = (myTeam != m_iTeamNum);

                                    if ((m_iHealth > 0) && (bDormant == false))
                                    {
                                        var headPos_fake = new Vector3(m_vecOrigin.X, m_vecOrigin.Y, m_vecOrigin.Z + f_modelHeight);
                                        Vector2 vScreen_head = new Vector2(0, 0);
                                        Vector2 vScreen_foot = new Vector2(0, 0);

                                        if (Renderer.WorldToScreen(headPos_fake, out vScreen_head, matrix, wndMargins, wndSize, W2SType.TypeD3D9))
                                        {
                                            Renderer.WorldToScreen(m_vecOrigin, out vScreen_foot, matrix, wndMargins, wndSize, W2SType.TypeD3D9);
                                            {
                                                string dist_str = "";
                                                if (Components.VisualsComponent.DrawTextDist.Enabled)
                                                {
                                                    double playerDist = GetDistance3D(myPos, m_vecOrigin) / 22.0f;
                                                    dist_str = $"[{playerDist.ToString("0.0")}]";
                                                }
                                                if (Components.VisualsComponent.DrawTheVisuals.Enabled)
                                                {
                                                    if ((!Components.VisualsComponent.DrawAlliesEsp.Enabled) && (!isenemybool)) continue;
                                                    Renderer.DrawFPSBox(vScreen_head, vScreen_foot, (m_iTeamNum == 3) ? Components.VisualsComponent.CTColor.Color : Components.VisualsComponent.TRColor.Color, (f_modelHeight == 54.0f) ? BoxStance.crouching : BoxStance.standing, Components.VisualsComponent.DrawBoxThic.Value, Components.VisualsComponent.DrawBoxBorder.Enabled, Components.VisualsComponent.DrawBox.Enabled, m_iHealth, Components.VisualsComponent.DrawBoxHP.Enabled ? 100 : 0, 0, 0, Components.VisualsComponent.DrawTextSize.Enabled ? Components.VisualsComponent.DrawTextSize.Value : 0, dist_str, string.Empty, string.Empty, string.Empty, string.Empty);
                                                }
                                            }
                                        }
                                        if (Components.AimbotComponent.AimGlobalBool.Enabled)
                                        {
                                            if (!Components.AimbotComponent.AimAtEveryone.Enabled)
                                            {
                                                if (!isenemybool) continue;
                                            }
                                            Vector3 targetVec = new Vector3(0, 0, 0);
                                            switch (Components.AimbotComponent.AimSpot.Value)
                                            {
                                                case 0:
                                                    {
                                                        targetVec = ReadBonePos(entityAddr, 8);
                                                    }
                                                    break;
                                                case 1:
                                                    {
                                                        targetVec = ReadBonePos(entityAddr, 0);
                                                    }
                                                    break;
                                                default:
                                                    break;
                                            }
                                            Vector2 vScreen_aim = new Vector2(0, 0);
                                            if (Renderer.WorldToScreen(targetVec, out vScreen_aim, matrix, wndMargins, wndSize, W2SType.TypeD3D9))
                                            {
                                                if (Components.AimbotComponent.DrawAimSpot.Enabled)
                                                {
                                                    Renderer.DrawFilledRect(vScreen_aim.X - 1, vScreen_aim.Y - 1, 2, 2, new Color(255, 255, 255));
                                                }
                                                var AimDist2D = GetDistance2D(vScreen_aim, GameCenterPos);
                                                if (Components.AimbotComponent.AimFov.Value < AimDist2D) continue;
                                                if (AimDist2D < fClosestPos)
                                                {
                                                    fClosestPos = AimDist2D;
                                                    AimTarg2D = vScreen_aim;
                                                    AimTarg3D = targetVec;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (Components.AimbotComponent.AimGlobalBool.Enabled)
                            {
                                if (Components.AimbotComponent.DrawAimFov.Enabled) //draw fov circle
                                {
                                    Renderer.DrawCircle(GameCenterPos, Components.AimbotComponent.AimFov.Value, Components.AimbotComponent.AimFovColor.Color);
                                }

                                var dx = (GameCenterPos.X * 2) / 90;
                                var dy = (GameCenterPos.Y * 2) / 90;
                                var rx = GameCenterPos.X - (dx * ((myPunchAngles.Y)));
                                var ry = GameCenterPos.Y + (dy * ((myPunchAngles.X)));

                                if (Components.AimbotComponent.DrawRecoil.Enabled)
                                {
                                    Renderer.DrawFilledRect(rx - 1, ry - 1, 2, 2, Components.AimbotComponent.RecoilColor.Color);
                                }

                                if ((AimTarg2D.X != 0) && (AimTarg2D.Y != 0))
                                {
                                    if (Components.AimbotComponent.DrawAimTarget.Enabled)
                                    {
                                        Renderer.DrawRect(AimTarg2D.X - 3, AimTarg2D.Y - 3, 6, 6, Components.AimbotComponent.AimTargetColor.Color);
                                    }
                                    if (Components.AimbotComponent.AimKey.Enabled)
                                    {

                                        if (Components.AimbotComponent.AimType.Value == 1)
                                        {
                                            {
                                                double DistX = 0;
                                                double DistY = 0;
                                                if (Components.AimbotComponent.AIMRC.Enabled)
                                                {
                                                    DistX = (AimTarg2D.X) - rx;
                                                    DistY = (AimTarg2D.Y) - ry;
                                                }
                                                else
                                                {
                                                    DistX = (AimTarg2D.X) - GameCenterPos.X;
                                                    DistY = (AimTarg2D.Y) - GameCenterPos.Y;
                                                }
                                                double slowDistX = DistX / (1.0f + (Math.Abs(DistX) / (1.0f + Components.AimbotComponent.AimSpeed.Value)));
                                                double slowDistY = DistY / (1.0f + (Math.Abs(DistY) / (1.0f + Components.AimbotComponent.AimSpeed.Value)));
                                                Input.mouse_eventWS(MouseEventFlags.MOVE, (int)slowDistX, (int)slowDistY, MouseEventDataXButtons.NONE, IntPtr.Zero);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
