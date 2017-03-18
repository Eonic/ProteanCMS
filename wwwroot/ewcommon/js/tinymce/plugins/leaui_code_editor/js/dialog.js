/**
 * leaui tinymce4 code editor
 * Copyright leaui.com
 */
 
// get data from top
var data = parent.LEAUI_CODE_EDITOR_DATAS;
var text = data.text;
var brush = data.brush;

var LEAUI_CODE_EDITOR;

$(function() {
	if(brush) {
		$("#lang").val(brush);
		$("#code").html(text);
	} else if(text) {
		$("#code").html(text);
	}

	/*
	// format code
	$("#format").click(function() {
		var code = $("#code").val();
		$("#oldCode").html(code);
		$("#code").val(js_beautify(code, 4, " "));
	});
	
	// cancel format
	$("#unFormat").click(function() {
		var oldCode = $("#oldCode").html();
		if(oldCode) {
			$("#code").val(oldCode);
		}
	});
	*/

	var m = {"cpp": "c_cpp", "bash": 'sh'};

	ace.require("ace/ext/language_tools");
	var editor = ace.edit("code");
	if(brush) {
		var lang = m[brush] || brush;
	} else {
		var lang = "text";
	}

	// ace editor configurations
	// more info please see http://ace.c9.io/#nav=howto

	editor.session.setMode("ace/mode/" + lang);
	editor.setTheme("ace/theme/tomorrow");
	// enable autocompletion and snippets
	editor.setOptions({
	    enableBasicAutocompletion: true,
	    enableSnippets: true,
	    enableLiveAutocompletion: false
	});

	LEAUI_CODE_EDITOR = editor;

	$("#lang").change(function() {
		var val = $(this).val();
		if(!val) {
			return;
		} else {
			val = m[val] || val;
		}
		editor.session.setMode("ace/mode/" + val);
	});
});