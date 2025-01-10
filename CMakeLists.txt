cmake_minimum_required(VERSION 3.5)
project(tlarc)

find_package(ament_cmake REQUIRED)

add_custom_target(run ALL)

add_custom_command(
  COMMAND dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true
          --self-contained true -o ./bin/pub/ TARGET run POST_BUILD
  WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR})


install(DIRECTORY configuration DESTINATION share/${PROJECT_NAME})
install(DIRECTORY maps DESTINATION share/${PROJECT_NAME})
install(DIRECTORY onnxModel DESTINATION share/${PROJECT_NAME})

file(GLOB ALL_RUNTIME_FILES Lib/runtimes)
file(GLOB ALL_LIB_FILES Lib)


install(DIRECTORY Lib/
        DESTINATION lib/${PROJECT_NAME}
        FILES_MATCHING PATTERN "*"
        )

install(DIRECTORY bin/pub/
        DESTINATION lib/${PROJECT_NAME}
        FILES_MATCHING PATTERN "*"
        )


install(
  FILES bin/pub/${PROJECT_NAME}
  DESTINATION lib/${PROJECT_NAME}
  PERMISSIONS OWNER_READ OWNER_WRITE OWNER_EXECUTE WORLD_EXECUTE)

ament_package()