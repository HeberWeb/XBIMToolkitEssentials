var check = Viewer.check();
var viewerFile = null;
var pickedId = null;
var countLoaded = 0;
var argTeste = null;

$(document).ready(function () {
    $('#testView').click(function () {
        viewer.ToggleLoading(true)
        viewer.controleSubMenu();
        viewer.initViewer();
    })
});

var classButton = 'adsk-button'
var classButtonIcon = 'adsk-button-icon'
var classControlTooltip = 'adsk-control-tooltip'

var viewer = {
    ToggleLoading: function(_trigger) {
        var callback = $.Deferred();
        if (_trigger) {
            $(".theme-loader").css("opacity", "0.7");
            $(".theme-loader").show();
            callback.resolve();
        } else {
            $(".theme-loader").hide();
            callback.resolve();
            $(".theme-loader").animate({ opacity: "0" }, "fast");
        }

        return callback
    },

    onLoadedFileOrFiles: function () {
        if (objViewer.Vincular) {
            if (countLoaded == objViewer.ListaIdsArquivosVincular.length) {
                viewer.ToggleLoading(false);
            }
        }
        else {
            viewer.ToggleLoading(false);
        }
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

            viewerFile.on('error', function (arg) {
                var container = document.getElementById('errors');
                if (container) {
                    //preppend error report
                    container.innerHTML = "<pre style='color:red;'>" + arg.message + "</pre><br/>" + container.innerHTML;
                }
                viewer.ToggleLoading(false);
            });

            viewerFile.on('pick', function (args) {
                console.log(args)
                var id = args.id;
                var span = document.getElementById('productId');
                if (span) {
                    span.innerHTML = id ? id : 'model';
                }
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

    initHiding: function () {
        viewerFile.on('pick', function (args) {
            var cmb = document.getElementById('showHide').getAttribute('value');
            switch (cmb) {
                case 'hideProduct':
                    viewerFile.setState(State.HIDDEN, [args.id]);
                    break;
                default:
                    break;
            }
        });
    },

    buttonSubMenu: function (_nameButton) {
        if ($('#' + _nameButton).hasClass('autodoc-hidden')) {

            // EXIBE O SUBMENU
            $(".toolbar-vertical-group").addClass("autodoc-hidden");
            $('#' + _nameButton).removeClass("autodoc-hidden");

        } else {
            // OCULTA SUBMENU
            $('#' + _nameButton).addClass("autodoc-hidden");
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
        document.getElementById('autodoc-body-viewer').addEventListener('click', function () {

            $(".toolbar-vertical-group").addClass("autodoc-hidden");
        });

        //// CLICK DENTRO DO MENU
        document.getElementById('toolbar-geral').addEventListener('click', function (event) {
            event.stopPropagation();
            // VERIFICO SE E UM SUBMENU
            var isSubmenu = $(event.target).closest('div.adsk-control').find('.toolbar-vertical-group');

            if (isSubmenu.length == 0) {
                // CASO NÂO, BUSCO E FECHO TODOS
                var subMenusList = $(".toolbar-vertical-group").addClass("autodoc-hidden");
            }
        });
    },

    markup: {
        openClose: function () {
            //toolbar-tools
            if (!$('#toolbar-markups').is(':visible')) {
                $('#toolbar-tools').fadeOut();
                $('#toolbar-markups').fadeIn();
                cvjs_loadStickyNotesRedlinesUser();
            } else {
                cvjs_deleteAllStickyNotes();
                cvjs_deleteAllRedlines();
                $('#toolbar-markups').fadeOut();
                $('#toolbar-tools').fadeIn();
            }
        },

        setColorMarkup: function (_this, _color) {
            $(_this).closest('#markup-colors').find('#markup-current-color').css('background-color', _color);
        },

        setCurrentAction: function (_this, _action, isComboButtun) {
            eval(_action + "('floorPlan_svg')")
            if (isComboButtun) {
                viewer.buttonSetActive(_this, 'toolbar-markups', "markup-basictools");
            } else {
                viewer.buttonSetActive(_this, 'toolbar-markups', "");
            }
        },

        deleteLastRedline: function () {
            if (ValidaExclusaoMarkup()) {
                cvjs_deleteLastRedline("floorPlan_svg")
            }
        }
    },


    backgroundColor: {
        getItemSubmenu: function (_this, hexa_cor) {

            switch (_this.id) {
                case "bg_preto":
                    //SETA COR PRETA DE FUNDO
                    cvjs_setBackgroundColorHex(hexa_cor, floorplan_div_Array[cvjs_active_floorplan_div_nr]);
                    break;
                case "bg_branco":
                    //SETA COR WHITE
                    cvjs_setBackgroundColorHex(hexa_cor, floorplan_div_Array[cvjs_active_floorplan_div_nr]);
                    break;
                case "bg_cinza":
                    //SETA COR CINZA
                    cvjs_setBackgroundColorHex(hexa_cor, floorplan_div_Array[cvjs_active_floorplan_div_nr]);
                    break;
            }

            //ALTERA O ÍCONE DO MENU
            //viewer.backgroundColor.setIconSelected(_this);
        },

        setIconSelected: function (_thisBtnSubMenu) {
            var btnSubMenu = $(_thisBtnSubMenu).closest('#background-tools').find('#bg-button-submenu');
            btnSubMenu.removeClass('autodoc-icon-background');
            btnSubMenu.toggleClass('autodoc-icon-' + _thisBtnSubMenu.id);
        }
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