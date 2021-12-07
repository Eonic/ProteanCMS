tinymce.PluginManager.add('ew-image', function (editor, url) {
    // Add a button that opens a window
    editor.ui.registry.addButton('ew-image', {
        text: 'My button',
        icon: false,
        onclick: function () {
            // Open window
            editor.windowManager.open({
                title: 'Example plugin',
                body: [
                    { type: 'textbox', name: 'title', label: 'Title' }
                ],
                onsubmit: function (e) {
                    // Insert content when the window form is submitted
                    editor.insertContent('Title: ' + e.data.title);
                }
            });
        }
    });

    // Adds a menu item to the tools menu
    editor.ui.registry.addMenuItem('ew-image', {
        text: 'Example plugin',
        context: 'tools',
        onclick: function () {
            // Open window with a specific url
            editor.windowManager.open({
                title: 'TinyMCE site',
                url: 'http://www.tinymce.com',
                width: 800,
                height: 600,
                buttons: [{
                    text: 'Close',
                    onclick: 'close'
                }]
            });
        }
    });
});