$number = 24

magick -size 128x128 xc:transparent -fill green -stroke white -strokewidth 2 -draw "roundrectangle 0,0 127,127 16,16" -gravity center -font Bahnschrift -fill white -pointsize 112 -annotate +0+5 "$number" $PSScriptRoot/../PomodoroScheduleNotifier/PomodoroScheduleNotifier/Resources/green-$number.ico