# gnuplot TEMPLATE for Table2D

# See template file for 3D surface plot, more documentation in there!

set termoption enhanced
set term wxt font "sans,14"

set grid

#plot "gnuplot_data.tmp" binary title "" with linespoints linetype 1 pt 7 linewidth 2

# two combined plots to allow separate colors for both lines and points
plot "gnuplot_data.tmp" binary volatile title "" with points linetype -1 pointtype 7 pointsize 1.0 linecolor rgb "blue", "gnuplot_data.tmp" binary volatile title "" with lines linetype 1 linewidth 3
