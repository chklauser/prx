
function PrxdocParam(parameterNode){
	parameterNode = $(parameterNode);
	this.name = parameterNode.children("th:first").get(0);
	this.description = parameterNode.children("td:first").get(0);
}

function PrxdocUsage(usageNode){
	usageNode = $(usageNode);
	this.syntax = usageNode.children("code:first").get(0);
	this.summary = usageNode.children("p:first").get(0);
	this.parameters = jQuery.map(jQuery.makeArray(usageNode.children("table:first").children("*:first").children("tr")),function(p) { return new PrxdocParam(p); });
	this.notes = usageNode.children("div:first").get(0);
}

function PrxdocCommand(commandNode){
	commandNode = $(commandNode);
	this.name = commandNode.attr("title");
	this.usages = jQuery.map(jQuery.makeArray(commandNode.children("ul:first").children("li")), function (u) { return new PrxdocUsage(u); });
    this.notes = commandNode.children("div:first");
}

function PrxdocSection(sectionNode){
	sectionNode = $(sectionNode);
	this.title = sectionNode.attr("title");
    var commands = { };
    sectionNode.children().each(function() {
        var cmd = new PrxdocCommand(this);
        commands[cmd.name] = cmd;
    });
    this.commands = commands;
}

function prxdoc_parse_reference(){
	return jQuery.map(jQuery.makeArray(jQuery("#command-reference section")),function(s){ return new PrxdocSection(s); });
}

var prxdoc_sections = {};

function display_command_from_link(sectionName,commandName) {
    var sct = prxdoc_sections[sectionName];
    var cmd = sct.commands[commandName];
    display_command(cmd);
}

function display_command(cmd) {
    var pageContent = $("#page_content");
    var commandTemplate = $("#command-template");
    var usageTemplate = commandTemplate.find("li:first");
    var parameterTemplate = usageTemplate.find("tr:first");

    pageContent.children().remove();
    var commandNode = commandTemplate.clone();

    commandNode.find(".template-command-name").replaceWith(cmd.name);

    var usageList = commandNode.children("ul:first");
    usageList.children().remove();

    cmd.usages.forEach(function (usage) {
        var node = usageTemplate.clone();
        node.find("tr").remove();
        node.find(".template-usage-syntax").replaceWith($(usage.syntax).contents().clone());
        node.find(".template-summary").replaceWith($(usage.summary).contents().clone());
        node.find(".template-notes").replaceWith($(usage.notes).contents().clone());
        node.find(".template-common-notes").replaceWith($(cmd.notes).contents().clone());
        var paramNodes = usage.parameters.map(function (param) {
            var pNode = parameterTemplate.clone();
            pNode.find(".template-parameter-name").replaceWith($(param.name).contents().clone());
            pNode.find(".template-parameter-description").replaceWith($(param.description).contents().clone());
            return pNode.get(0);
        });
        node.find(".template-parameters > tbody").append(paramNodes);
        node.find(".template-parameters").removeClass("template-parameters");
        usageList.append(node);
    });

    pageContent.append(commandNode.contents());
}

$(document).ready(function () {
    var leftcol = jQuery("#left_col");
    jQuery.map(prxdoc_parse_reference(), function (sct) {
        prxdoc_sections[sct.title] = sct;
        var sctName = $(document.createElement("h3"));
        var sctLink = $(document.createElement("a"));
        sctLink.attr("href", "#" + sct.title + "-");
        sctLink.text(sct.title);
        sctName.append(sctLink);
        leftcol.append(sctName);

        var sctElem = $(document.createElement("div"));
        var cmdList = $(document.createElement("ul"));
        for (var cmdName in sct.commands) {
            function body(cmd) { // need to capture cmd by value.
                var liElem = jQuery(document.createElement("li"));
                var link = jQuery(document.createElement("a"));
                link.attr("href", "#" + sct.title + "-" + cmd.name);
                link.bind("click", null, function () {
                    display_command(cmd);
                });
                link.text(cmd.name);
                liElem.append(link);
                cmdList.append(liElem);
            }
            body(sct.commands[cmdName]);
        };
        sctElem.append(cmdList);
        leftcol.append(sctElem);
    });

    $("#left_col").accordion();

    //Interpret anchor as URL
    var url = document.location.toString();
    if (url.match('#')) { // the URL contains an anchor
        // click the navigation item corresponding to the anchor
        var topic = '#' + url.split('#')[1];
        var dashIdx = topic.lastIndexOf("-");
        var section = topic.substring(1, dashIdx);
        $("#left_col").children("h3").each(function (idx, h3) {
            if ($(h3).find(' a[href="#' + section + '-"]').length > 0) {
                $("#left_col").accordion("activate", idx);
            }
        });
        $('div#left_col > div.ui-accordion-content > ul > li > a[href="' + topic + '"]').click();
    } else {
        // display default topic
    }
});
