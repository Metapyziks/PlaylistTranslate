premake.override(path, "translate", function (base, p)
    return p
end)

solution "PlaylistTranslate"
    configurations { "Debug", "Release" }
    
    framework "4.5"

    configuration "Debug"
        defines { "DEBUG", "TRACE" }
        flags { "Symbols" }
        optimize "Off"
        targetdir "bin/Debug"

    configuration "Release"
        defines { "TRACE" }
        optimize "On"
        targetdir "bin/Release"

    project "PlaylistTranslate"
        kind "ConsoleApp"
        language "C#"
        location "PlaylistTranslate"
        files { "PlaylistTranslate/**.cs" }
        excludes { "PlaylistTranslate/obj/**.cs" }
        links { "System", "System.Core", "System.Web", "System.Data", "Newtonsoft.Json.dll" }
        postbuildcommands { "cp \"%{cfg.project.location}/Newtonsoft.Json.dll\" \"%{cfg.targetdir}/Newtonsoft.Json.dll\"" }
