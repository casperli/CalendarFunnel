/*
@license
dhtmlxScheduler v.4.3.1 

This software is covered by GPL license. You also can obtain Commercial or Enterprise license to use it in non-GPL project - please contact sales@dhtmlx.com. Usage without proper license is prohibited.

(c) Dinamenta, UAB.
*/
scheduler.__recurring_template='<div class="dhx_form_repeat"> <form> <div class="dhx_repeat_left"> <label><input class="dhx_repeat_radio" type="radio" name="repeat" value="day" />Diário</label><br /> <label><input class="dhx_repeat_radio" type="radio" name="repeat" value="week"/>Semanal</label><br /> <label><input class="dhx_repeat_radio" type="radio" name="repeat" value="month" checked />Mensal</label><br /> <label><input class="dhx_repeat_radio" type="radio" name="repeat" value="year" />Anual</label> </div> <div class="dhx_repeat_divider"></div> <div class="dhx_repeat_center"> <div style="display:none;" id="dhx_repeat_day"> <label><input class="dhx_repeat_radio" type="radio" name="day_type" value="d"/>Cada</label><input class="dhx_repeat_text" type="text" name="day_count" value="1" />dia(s)<br /> <label><input class="dhx_repeat_radio" type="radio" name="day_type" checked value="w"/>Cada trabalho diário</label> </div> <div style="display:none;" id="dhx_repeat_week"> Repita cada<input class="dhx_repeat_text" type="text" name="week_count" value="1" />semana:<br /> <table class="dhx_repeat_days"> <tr> <td> <label><input class="dhx_repeat_checkbox" type="checkbox" name="week_day" value="1" />Segunda</label><br /> <label><input class="dhx_repeat_checkbox" type="checkbox" name="week_day" value="4" />Quinta</label> </td> <td> <label><input class="dhx_repeat_checkbox" type="checkbox" name="week_day" value="2" />Terça</label><br /> <label><input class="dhx_repeat_checkbox" type="checkbox" name="week_day" value="5" />Sexta</label> </td> <td> <label><input class="dhx_repeat_checkbox" type="checkbox" name="week_day" value="3" />Quarta</label><br /> <label><input class="dhx_repeat_checkbox" type="checkbox" name="week_day" value="6" />Sábado</label> </td> <td> <label><input class="dhx_repeat_checkbox" type="checkbox" name="week_day" value="0" />Domingo</label><br /><br /> </td> </tr> </table> </div> <div id="dhx_repeat_month"> <label><input class="dhx_repeat_radio" type="radio" name="month_type" value="d"/>Repetir</label><input class="dhx_repeat_text" type="text" name="month_day" value="1" />todo dia<input class="dhx_repeat_text" type="text" name="month_count" value="1" />mês<br /> <label><input class="dhx_repeat_radio" type="radio" name="month_type" checked value="w"/>Em</label><input class="dhx_repeat_text" type="text" name="month_week2" value="1" /><select name="month_day2"><option value="1" selected >Segunda<option value="2">Terça<option value="3">Quarta<option value="4">Quinta<option value="5">Sexta<option value="6">Sábado<option value="0">Domingo</select>todo<input class="dhx_repeat_text" type="text" name="month_count2" value="1" />mês<br /> </div> <div style="display:none;" id="dhx_repeat_year"> <label><input class="dhx_repeat_radio" type="radio" name="year_type" value="d"/>Todo</label><input class="dhx_repeat_text" type="text" name="year_day" value="1" />dia<select name="year_month"><option value="0" selected >Janeiro<option value="1">Fevereiro<option value="2">Março<option value="3">Abril<option value="4">Maio<option value="5">Junho<option value="6">Julho<option value="7">Agosto<option value="8">Setembro<option value="9">Outubro<option value="10">Novembro<option value="11">Dezembro</select>mês<br /> <label><input class="dhx_repeat_radio" type="radio" name="year_type" checked value="w"/>Em</label><input class="dhx_repeat_text" type="text" name="year_week2" value="1" /><select name="year_day2"><option value="1" selected >Segunda<option value="2">Terça<option value="3">Quarta<option value="4">Quinta<option value="5">Sexta<option value="6">Sábado<option value="7">Domingo</select>of<select name="year_month2"><option value="0" selected >Janeiro<option value="1">Fevereiro<option value="2">Março<option value="3">Abril<option value="4">Maio<option value="5">Junho<option value="6">Julho<option value="7">Agosto<option value="8">Setembro<option value="9">Outubro<option value="10">Novembro<option value="11">Dezembro</select><br /> </div> </div> <div class="dhx_repeat_divider"></div> <div class="dhx_repeat_right"> <label><input class="dhx_repeat_radio" type="radio" name="end" checked/>Sem data final</label><br /> <label><input class="dhx_repeat_radio" type="radio" name="end" />Depois</label><input class="dhx_repeat_text" type="text" name="occurences_count" value="1" />ocorrências<br /> <label><input class="dhx_repeat_radio" type="radio" name="end" />Fim</label><input class="dhx_repeat_date" type="text" name="date_of_end" value="'+scheduler.config.repeat_date_of_end+'" /><br /> </div> </form> </div> <div style="clear:both"> </div>';

//# sourceMappingURL=../../sources/locale/recurring/locale_recurring_pt.js.map