var check = Viewer.check();
var viewerFile = null;
var pickedId = null;
var countLoaded = 0;
var argTeste = null;

$(document).ready(function () {
    $('#testView').click(function () {
        viewer.controleSubMenu();
        viewer.initViewer();
    })
});

var classButton = 'adsk-button'
var classButtonIcon = 'adsk-button-icon'
var classControlTooltip = 'adsk-control-tooltip'

var viewer = {
    onLoadedFileOrFiles: function () {

    },

    initViewer: function () {
        countLoaded = 0;
        if (check.noErrors) {
            viewerFile = new Viewer('viewer');

            viewerFile.on('pick', function (args) {
                var id = args.id;
                pickedId = id;
            });

            viewerFile.on('loaded', function (args) {
                //viewer.initHiding();
                viewerFile.start();
                viewerFile.show(ViewType.DEFAULT)

                countLoaded++;
                viewer.onLoadedFileOrFiles();
                argTeste = args;
                
            });

            viewerFile.on('fps', function (fps) {
                var spanFps = document.getElementById('spanFps')

                if (spanFps) {
                    spanFps.innerHTML = fps
                }
            });

            viewerFile.on('error', function (arg) {
                var container = document.getElementById('errors');
                if (container) {
                    //preppend error report
                    container.innerHTML = "<pre style='color:red;'>" + arg.message + "</pre><br/>" + container.innerHTML;
                }
            });

            viewerFile.on('pick', function (args) {
                if (args == null || args.id == null) {
                    return;
                }
                console.log(args);
                var productId = args.id;
                var modelId = args.model;
                var coordsId = `[${Array.from(args.xyz).map(x => x.toFixed(2))}]`

                var spanProductId = document.getElementById('productId');
                var spanModelId = document.getElementById('modelId');
                var spanCoordsId = document.getElementById('coordsId');
                if (spanProductId) {
                    spanProductId.innerHTML = productId ? productId : 'model spanProductId';
                }
                if (spanModelId) {
                    spanModelId.innerHTML = modelId ? modelId : 'model spanModelId';
                }
                if (spanCoordsId) {
                    spanCoordsId.innerHTML = coordsId ? coordsId : 'model spanCoordsId';
                }

                if (viewerFile.getState([args.id]) == State.HIGHLIGHTED) {
                    viewerFile.setState(State.UNDEFINED, [args.id])
                } else {
                    viewerFile.setState(State.HIGHLIGHTED, [args.id])
                }
                
            });

            viewerFile.on('dblclick', function (args) {
                if (args == null || args.id == null) {
                    return;
                }

                viewerFile.zoomTo(args.id)
            });

            viewerFile
                .load("Home/RetornoArquivoIFC?file=" + $('#nameFile').val());
        }
        else {
            var msg = document.getElementById('errors');
            for (var i in check.errors) {
                var error = check.errors[i];
                msg.innerHTML += "<pre style='color: red;'>" + error + "</pre><br />";
            }
        }
    },

    buttonSubMenu: function (_nameButton) {
        if ($('#' + _nameButton).hasClass('xbimTest-hidden')) {

            // EXIBE O SUBMENU
            $(".toolbar-vertical-group").addClass("xbimTest-hidden");
            $('#' + _nameButton).removeClass("xbimTest-hidden");

        } else {
            // OCULTA SUBMENU
            $('#' + _nameButton).addClass("xbimTest-hidden");
        }
    },

    buttonSetActive: function (_this, _toolbarId, _comboButtonId) {
        var $this = $(_this);
        var removeActive = $('#' + _toolbarId).find('.' + classButton);

        setTimeout(function () {

            if (_comboButtonId != "") {
                var comboButtonId = $("#" + _comboButtonId);
                if (comboButtonId.length > 0) {
                    var classButtonThis = $this.find('.' + classButtonIcon).attr('class');
                    var tooltipThis = $this.find('.' + classControlTooltip).text();

                    var ButtonMain = comboButtonId.children('.' + classButtonIcon)
                    var TooltipMain = comboButtonId.children('.' + classControlTooltip).text(tooltipThis).attr('tooltiptext', tooltipThis)

                    var classButtonMainSet = ButtonMain.attr('class')
                    ButtonMain.removeClass(classButtonMainSet)
                    ButtonMain.addClass(classButtonThis)
                }
            }
        }, 30);
    },

    controleSubMenu: function () {

        //// CLICK FORA DO MENU
        document.getElementById('xbimTest-body-viewer').addEventListener('click', function () {

            $(".toolbar-vertical-group").addClass("xbimTest-hidden");
        });

        //// CLICK DENTRO DO MENU
        document.getElementById('toolbar-geral').addEventListener('click', function (event) {
            event.stopPropagation();
            // VERIFICO SE E UM SUBMENU
            var isSubmenu = $(event.target).closest('div.adsk-control').find('.toolbar-vertical-group');

            if (isSubmenu.length == 0) {
                // CASO NÂO, BUSCO E FECHO TODOS
                var subMenusList = $(".toolbar-vertical-group").addClass("xbimTest-hidden");
            }
        });
    },

    setCamera(_this) {

        var camType = document.getElementById('vista-ortogonal');

        if (camType.value == "1") {
            $("#tooltip-vista-ortogonal").empty();
            $(_this).css('background-color', "#222222f0");
            $("#tooltip-vista-ortogonal").text("Perspectiva");
            viewerFile.camera = parseInt(camType.value);
            camType.value = "0";

        } else {
            $("#tooltip-vista-ortogonal").empty();
            $(_this).css('background-color', "#2e2e2e");
            $("#tooltip-vista-ortogonal").text("Ortogonal");
            viewerFile.camera = parseInt(camType.value);
            camType.value = "1";
        }
    },

    setTransparencia(_this) {

        var camType = document.getElementById('transparencia');

        if (camType.value == "1") {
            $("#tooltip-transparencia").empty();
            $(_this).css('background-color', "#222222f0");
            $("#tooltip-transparencia").text("Transparência");
            viewerFile.renderingMode = RenderingMode.XRAY_ULTRA;
            camType.value = "0";

        } else {
            $("#tooltip-transparencia").empty();
            $(_this).css('background-color', "#2e2e2e");
            $("#tooltip-transparencia").text("Normal");
            viewerFile.renderingMode = RenderingMode.NORMAL;
            camType.value = "1";
        }
    },
};