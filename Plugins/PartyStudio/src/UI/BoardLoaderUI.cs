using ImGuiNET;
using MapStudio.UI;
using MPLibrary;
using Newtonsoft.Json;
using PartyStudioPlugin.src.UI;
using System.Collections.Generic;
using System.IO;
using Toolbox.Core;
using System;
using System.Linq;

namespace PartyStudioPlugin
{
    public class BoardLoaderUI
    {
        public GameList Games = new GameList();

        public BoardLoaderUI()
        {
            Games.Export();
            Games.Import();
        }

        public GameConfig ActiveGame;

        public string SelectedFile;

        public string FilePath => Path.Combine(ActiveGame.GamePath, SelectedFile);

        public static event Action<string> OnLoadBoardRequested;

        public void Render()
        {
            if (ImGui.BeginChild("game_list", new System.Numerics.Vector2(ImGui.GetWindowWidth(), 23)))
            {
                ImGui.Columns(1);
                foreach (GameConfig game in Games.List)
                {
                    if (game.Version == GameVersion.MP4)
                    {
                        if (ImGui.RadioButton(game.Version.ToString(), game == ActiveGame))
                            ActiveGame = game;
                    }
                }
                ImGui.Columns(1);
            }
            ImGui.EndChild();

            if (ImGui.BeginChild("file_list", new System.Numerics.Vector2(ImGui.GetWindowWidth(), ImGui.GetWindowHeight() - 100)))
            {
                if (ActiveGame != null)
                {
                    string folder = ActiveGame.GamePath;
                    if (ImguiCustomWidgets.PathSelector("Data Folder", ref folder))
                    {
                        ActiveGame.GamePath = folder;
                        ActiveGame.SaveConfig(); //save config
                    }

                    ImGui.Columns(2);

                    ImGuiHelper.BoldText("File Name"); ImGui.NextColumn();
                    ImGuiHelper.BoldText("Name"); ImGui.NextColumn();

                    foreach (var file in ActiveGame.BoardFileList)
                    {
                        bool selected = SelectedFile == file.Value;
                        if (ImGui.Selectable(file.Value, selected, ImGuiSelectableFlags.SpanAllColumns))
                        {
                            SelectedFile = file.Value;
                            if (file.Key.StartsWith("w"))
                            {
                                LoadBoard();
                            }
                        }
                        if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(0))
                        {
                            SelectedFile = file.Value;
                            LoadBoard();
                        }

                        ImGui.NextColumn();
                        ImGui.Text(file.Key);
                        ImGui.NextColumn();
                    }
                    ImGui.Columns(1);
                }
            }
            ImGui.EndChild();

            DialogHandler.DrawCancelOk();
        }

        public void LoadBoard()
        {
            if (string.IsNullOrEmpty(SelectedFile) || ActiveGame == null)
            {
                Console.WriteLine("LoadBoard: No file selected or no active game.");
                return;
            }

            string path = Path.Combine(ActiveGame.GamePath, $"{SelectedFile}.bin");
            if (!File.Exists(path))
            {
                Console.WriteLine($"LoadBoard: File not found at path: {path}");
                return;
            }

            Console.WriteLine($"LoadBoard: Attempting to load board from path: {path}");
            // Invoke the event to request loading the board
            OnLoadBoardRequested?.Invoke(path);

            // Set the active workspace tool to the board editor
            if (Workspace.ActiveWorkspace != null)
            {
                var boardEditorTool = Workspace.ActiveWorkspace.WorkspaceTools.FirstOrDefault(tool => tool.Header.Contains("Map Editor"));
                if (boardEditorTool != null)
                {
                    Workspace.ActiveWorkspace.ActiveWorkspaceTool = boardEditorTool;
                }
            }

            // Close the dialog
            DialogHandler.ClosePopup(true);
        }
    }
}
