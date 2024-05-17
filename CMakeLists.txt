cmake_minimum_required(VERSION 3.5)
project(decision_maker)

find_package(ament_cmake REQUIRED)

add_custom_target(run ALL)

add_custom_command(
    COMMAND
    dotnet publish -r linux-x64 -c Release -p:PublishSingleFile=true --self-contained false -o ./bin/
    TARGET run POST_BUILD
    WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
)

install(FILES
    bin/${PROJECT_NAME}
    DESTINATION lib/${PROJECT_NAME}
    PERMISSIONS WORLD_EXECUTE 
)
install(DIRECTORY Declaration
    DESTINATION share/
)

ament_package()