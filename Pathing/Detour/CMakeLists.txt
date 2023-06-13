cmake_minimum_required(VERSION 3.00)
project(DOL_Detour)

file(GLOB SOURCES Source/*.cpp)
add_library(dol_detour SHARED ${SOURCES})

add_library(RecastNavigation::Detour ALIAS dol_detour)
set_target_properties(dol_detour PROPERTIES DEBUG_POSTFIX -d)

set(dol_detour_INCLUDE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/Include")

target_include_directories(dol_detour PUBLIC
    "$<BUILD_INTERFACE:${dol_detour_INCLUDE_DIR}>"
)

find_package(Threads)
target_link_libraries(dol_detour ${CMAKE_THREAD_LIBS_INIT})

set_target_properties(dol_detour PROPERTIES
        SOVERSION ${SOVERSION}
        VERSION ${LIB_VERSION}
        COMPILE_PDB_OUTPUT_DIRECTORY .
        COMPILE_PDB_NAME "dol_detour-d"
        )

install(TARGETS dol_detour
        RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
        ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
        LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
        COMPONENT library
        )

file(GLOB INCLUDES Include/*.h)
install(FILES ${INCLUDES} DESTINATION
    ${CMAKE_INSTALL_INCLUDEDIR}/recastnavigation)
if(MSVC)
    install(FILES "$<TARGET_FILE_DIR:dol_detour>/dol_detour-d.pdb" CONFIGURATIONS "Debug" DESTINATION "lib")
endif()

# Tests
file(GLOB TEST_SOURCES Test/*.cpp)
file(GLOB NAVS Test/*.nav)
add_executable(detour_test ${TEST_SOURCES})
set(detour_test "${CMAKE_CURRENT_SOURCE_DIR}/Include")
file(COPY ${NAVS} DESTINATION ${CMAKE_CURRENT_BINARY_DIR})
target_compile_features(detour_test PRIVATE cxx_std_17)
target_link_libraries(detour_test dol_detour ${CMAKE_THREAD_LIBS_INIT})
add_test(NAME detour_test COMMAND detour_test)
