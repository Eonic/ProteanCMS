/**
 * leaui tinymce4 code editor
 * http://leaui.com
 */
var LEAUI_CODE_EDITOR_DATAS = {brush:"", text: "", node: null};
tinymce.PluginManager.add('leaui_code_editor', function(editor, url) {
	var ed = editor;
	var dom;

	function isBrush() {
		LEAUI_CODE_EDITOR_DATAS = {brush:"", text: "", node: null};
		dom = editor.dom;
		var text = "";
		var node = editor.selection.getNode();
		// console.log(node);
		var selectedContent = editor.selection.getContent();
		if(node.nodeName == 'PRE') {
			try {
				LEAUI_CODE_EDITOR_DATAS.brush = dom.getAttrib(node, 'class').split(":")[1];
				text = node.innerHTML;
				LEAUI_CODE_EDITOR_DATAS.node = node;
			} catch(e) {
			}
		} else if(selectedContent) {
			try {
				var newNode = document.createElement("p");
				p.innerHTML = p;
				text = p.innerText;
			} catch(e) {
			}
			if(!text) {
				text = selectedContent;
			}
		}
		LEAUI_CODE_EDITOR_DATAS.text = text;
	}
	// v2.0
	function htmlEnCode(str){  
        var    s    =    "";  
        if    (str.length    ==    0)    return    "";  
        s    =    str.replace(/&/g,    "&gt;");  
        s    =    s.replace(/</g,        "&lt;");  
        s    =    s.replace(/>/g,        "&gt;");  
        // s    =    s.replace(/ /g,        "&nbsp;");  
        // s    =    s.replace(/\'/g,      "'");  
        // s    =    s.replace(/\"/g,      "&quot;");  
        return    s;  
	}

	ed.addCommand('leaui_code_editor', function() {
		isBrush();
		// console.log(LEAUI_CODE_EDITOR_DATAS);

		ed.windowManager.open({
			title: "Insert code",
			// file : url + '/dialog.htm',
			html: '<iframe id="leauiCodesyntaxIfr" src="'+ url + '/dialog.htm'+ '?' + new Date().getTime() + '" frameborder="0"></iframe>',
			width : 600,
			height : 320,
			buttons: [{
				text: 'Insert Code',
				subtype: 'primary',
				onclick: function(e) {
					var _iframe = document.getElementById('leauiCodesyntaxIfr').contentWindow;
					var langO =_iframe.document.getElementById('lang');
					// var codeO =_iframe.document.getElementById('code');
					var lang = langO.value;
					// var code = codeO.value;
					var code = _iframe.LEAUI_CODE_EDITOR.session.getValue();
					// encode code
					console.log(code);
					code = htmlEnCode(code);
					console.log(code);
					var classes = "";
					if(lang) {
						classes = "brush:" + lang;
					}
					var html = '<pre class="' + classes + '">' + code + "</pre>";
					// overwrite
					if(LEAUI_CODE_EDITOR_DATAS.node) {
						dom.setAttrib(LEAUI_CODE_EDITOR_DATAS.node, "class", classes);
						LEAUI_CODE_EDITOR_DATAS.node.innerHTML = code;
					} else {
						editor.insertContent(html);
					}

					this.parent().parent().close();
				}
			},
			{
				text: 'Cancel',
				onclick: function(e) {
					this.parent().parent().close();
				}
			}]

		});
	});
	ed.addButton('leaui_code_editor', {
		title : 'Insert Code',
		cmd : 'leaui_code_editor',
		icon: "code",
		stateSelector: 'pre'
	});
	editor.addMenuItem('leaui_code_editor', {
		icon: 'code',
		text: 'Insert Code',
		cmd : 'leaui_code_editor',
		context: 'insert',
		stateSelector: 'pre',
		prependToContext: true
	});
	
	//-------------------
    // short cut
    function replaceNode(oldNode, newNodeHtml) {
		var p = document.createElement("p");
		p.innerHTML = newNodeHtml;
		var newNode = p.childNodes[0];
		oldNode.parentNode.replaceChild(newNode, oldNode);
	}
	function getSelectedText(selectedContent){
		var p = document.createElement("p");
		p.innerHTML = selectedContent;
		return p.innerText;
	}
	function trim(str) {
	    return str.replace(/^\s\s*/, '').replace(/\s\s*$/, '');
	}
	ed.addCommand('toggleCode', function() {
		var node = ed.selection.getNode();
		var selectedContent = ed.selection.getContent();
		var everBookmark = ed.selection.getBookmark();
		var text;
		try {
			text = trim(getSelectedText(selectedContent));
		} catch(e) {
		}
		// maybe it isn't a complete html, so text == null
		if(!text) {
			text = trim(selectedContent);
		}
		if(text) {
			if(node.nodeName == "PRE") {
				replaceNode(node, "<p>" + $(node).html() + "</p>");
			} else {
				ed.insertContent("<pre>" + text + "</pre>");
			}
		} else {
			// toggle current line to pre
			if(node.nodeName == "PRE") {
				replaceNode(node, "<p>" + node.innerHTML.replace(/\n/g, "<br />") + "</p>");
			} else {
				replaceNode(node, "<pre>" + node.innerHTML + "</pre>");
			}
		}
		ed.selection.moveToBookmark(everBookmark);
	});
	// shortcuts
    ed.addShortcut('ctrl+shift+c', '', 'toggleCode');
	ed.addShortcut('command+shift+c', '', 'toggleCode');
});
