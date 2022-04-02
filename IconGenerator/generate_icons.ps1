# Parameter arrays, columns are corresponding
$color_labels = @("red", "green", "blue")
$color_hexes = @("#fe0052", "#00a807", "#0078fa")
$max_nums = @(25, 5, 35)

for ($color_i = 0; $color_i -lt $color_labels.length; $color_i++) {
    $color_label = $color_labels[$color_i]
    $color_hex = $color_hexes[$color_i]
    $max_num = $max_nums[$color_i]

    for ($number = 1; $number -le $max_num; $number++) {
        magick -size 128x128 xc:transparent -fill $color_hex -stroke white -strokewidth 2 -draw "roundrectangle 0,0 127,127 16,16" -gravity center -font Bahnschrift -fill white -pointsize 112 -strokewidth 0 -annotate +0+5 "$number" $PSScriptRoot/../PomodoroScheduleNotifier/PomodoroScheduleNotifier/Resources/$color_label-$number.ico
    }
}