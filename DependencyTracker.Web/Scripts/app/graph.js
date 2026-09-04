/* DependencyTracker Cytoscape.js visualization */
window.DTCy = window.DTCy || {};

(function (cyModule) {
    'use strict';

    var cfg = window.DTConfig;
    var $ = window.jQuery;

    var state = {
        cy: null,
        container: null,
        rootApplicationId: null,
        allMode: false,
        currentDepth: 3,
        currentDirection: 'Both',
        expandedNodes: {},
        canEdit: false,
        detailsUrlTemplate: null,
        graphUrl: '/api/Graph',
        filters: {
            environment: {},
            criticality: {},
            status: {}
        }
    };

    /* ----------------------------------------------------------------------
       Style definitions
       -------------------------------------------------------------------- */
    function buildStyle() {
        return [
            {
                selector: 'node',
                style: {
                    'label': function (ele) {
                        return ele.data('name');
                    },
                    'text-wrap': 'wrap',
                    'text-max-width': '90px',
                    'text-valign': 'center',
                    'text-halign': 'center',
                    'text-margin-y': 2,
                    'shape': 'roundrectangle',
                    'width': 'label',
                    'min-width': 46,
                    'padding': '10px',
                    'height': 34,
                    'font-size': 11,
                    'font-weight': 'bold',
                    'border-width': 3,
                    'border-color': function (ele) {
                        return cfg.criticalityColor(ele.data('criticality'));
                    },
                    'background-color': function (ele) {
                        return cfg.levelColor(ele.data('level'));
                    },
                    'color': function (ele) {
                        return cfg.nodeTextColor(cfg.levelColor(ele.data('level')));
                    }
                }
            },
            {
                selector: 'node[?isRoot]',
                style: {
                    'shape': 'roundrectangle',
                    'width': 'label',
                    'height': 42,
                    'font-size': 13,
                    'border-width': 4,
                    'border-color': '#1d3557',
                    'z-index': 10
                }
            },
            {
                selector: 'node.pressed',
                style: {
                    'border-width': 5,
                    'border-color': '#ffffff',
                    'transition-property': 'border-width',
                    'transition-duration': '150ms'
                }
            },
            {
                selector: 'node[?isRetired]',
                style: {
                    'opacity': 0.5
                }
            },
            {
                selector: 'edge',
                style: {
                    'width': 2,
                    'line-color': '#adb5bd',
                    'target-arrow-color': '#adb5bd',
                    'target-arrow-shape': 'triangle',
                    'curve-style': 'bezier',
                    'label': function (ele) {
                        return cfg.edgeLabel(ele.data());
                    },
                    'text-rotation': 'autorotate',
                    'font-size': 8,
                    'text-background-color': '#ffffff',
                    'text-background-opacity': 0.75,
                    'text-background-padding': '2px',
                    'text-margin-x': 2
                }
            },
            {
                selector: 'edge[criticality = "Critical"]',
                style: {
                    'width': 3.5,
                    'line-color': '#e63946',
                    'target-arrow-color': '#e63946'
                }
            },
            {
                selector: 'edge[criticality = "High"]',
                style: {
                    'width': 2.5,
                    'line-color': '#f4a261',
                    'target-arrow-color': '#f4a261'
                }
            },
            {
                selector: 'edge[criticality = "Medium"]',
                style: {
                    'width': 2,
                    'line-color': '#8ecae6',
                    'target-arrow-color': '#8ecae6'
                }
            },
            {
                selector: 'edge[criticality = "Low"]',
                style: {
                    'width': 1.5,
                    'line-color': '#adb5bd',
                    'target-arrow-color': '#adb5bd'
                }
            },
            {
                selector: 'edge.lvl-1',
                style: {
                    'line-style': 'solid'
                }
            },
            {
                selector: '.faded',
                style: {
                    'opacity': 0.15
                }
            },
            {
                selector: '.highlighted-node',
                style: {
                    'border-width': 6,
                    'border-color': '#ffd166',
                    'z-index': 100
                }
            }
        ];
    }

    /* ----------------------------------------------------------------------
       Graph data loading
       -------------------------------------------------------------------- */
    function loadGraph(applicationId, direction, depth) {
        showLoading(true);
        $('#graph-empty').addClass('d-none');
        state.currentDepth = depth;
        state.currentDirection = direction;
        state.expandedNodes = {};

        var isAll = applicationId === 'all' || applicationId === null || applicationId === undefined;
        state.allMode = isAll;
        state.rootApplicationId = isAll ? null : applicationId;

        var params = { depth: depth, direction: direction };
        if (!isAll) params.applicationId = applicationId;

        $.ajax({
            url: state.graphUrl,
            data: params,
            dataType: 'json',
            cache: false
        })
            .done(function (data) {
                if (!data || !data.nodes) {
                    showLoading(false);
                    return;
                }
                renderGraph(data);
            })
            .fail(function (xhr) {
                showLoading(false);
                showToast('Failed to load graph data.', 'danger');
                if (xhr.status === 403) {
                    window.location.href = '/Home/Index';
                }
            });
    }

    function renderGraph(data) {
        var nodes = $.map(data.nodes, function (n) {
            return {
                data: {
                    id: n.id,
                    name: n.name,
                    level: n.level,
                    isRoot: n.isRoot,
                    criticality: n.criticality,
                    environment: n.environment,
                    status: n.status,
                    applicationId: n.applicationId,
                    isRetired: n.status === 'Retired'
                }
            };
        });

        var edges = $.map(data.edges, function (e) {
            return {
                data: {
                    id: e.id,
                    source: e.source,
                    target: e.target,
                    type: e.type,
                    criticality: e.criticality,
                    impact: e.impact,
                    frequency: e.frequency,
                    direction: e.direction,
                    level: e.level,
                    dependencyId: e.dependencyId
                }
            };
        });

        // Build elements and set up the graph (or reuse existing instance)
        if (!state.cy) {
            state.cy = window.cytoscape({
                container: state.container,
                elements: { nodes: nodes, edges: edges },
                style: buildStyle(),
                wheelSensitivity: 0.2,
                minZoom: 0.05,
                maxZoom: 3,
                layout: { name: 'preset', fit: false }
            });
            bindInteractions();
        } else {
            state.cy.elements().remove();
            state.cy.add({ nodes: nodes, edges: edges });
        }

        runGraphLayout(state.cy);
        state.cy.fit(undefined, 40);
        showLoading(false);
        updateLevelLegend(nodes);
        populateFilters(nodes);
    }

    /* ----------------------------------------------------------------------
       Filtering (client-side, by environment / criticality / status)
       -------------------------------------------------------------------- */
    function populateFilters(nodes) {
        var envs = {};
        var crits = {};
        var statuses = {};

        $.each(nodes, function (_, n) {
            var d = n.data;
            if (d.environment) envs[d.environment] = true;
            if (d.criticality) crits[d.criticality] = true;
            if (d.status) statuses[d.status] = true;
        });

        renderFilterGroup('#filter-environment', Object.keys(envs).sort(), state.filters.environment);
        renderFilterGroup('#filter-criticality', Object.keys(crits).sort(), state.filters.criticality);
        renderFilterGroup('#filter-status', Object.keys(statuses).sort(), state.filters.status);
    }

    function renderFilterGroup(containerSelector, values, selection) {
        var $container = $(containerSelector);
        $container.empty();
        $.each(values, function (_, value) {
            if (selection[value] === undefined) {
                selection[value] = true; // default to selected
            }
            $container.append(
                '<label class="filter-check">' +
                '<input type="checkbox" data-value="' + value + '" ' + (selection[value] ? 'checked' : '') + ' /> ' +
                '<span>' + value + '</span>' +
                '</label>'
            );
        });
        $container.find('input[type="checkbox"]').on('change', function () {
            var value = $(this).data('value');
            selection[value] = $(this).is(':checked');
            applyFilters();
        });
    }

    function applyFilters() {
        if (!state.cy) return;

        var env = state.filters.environment;
        var crit = state.filters.criticality;
        var status = state.filters.status;

        var hasEnv = $.map(env, function (v) { return v; }).some(function (v) { return v; });
        var hasCrit = $.map(crit, function (v) { return v; }).some(function (v) { return v; });
        var hasStatus = $.map(status, function (v) { return v; }).some(function (v) { return v; });

        function selected(map, value, anySelected) {
            if (!anySelected) return true;
            return map[value] === true;
        }

        state.cy.nodes().forEach(function (node) {
            var d = node.data();
            var visible =
                selected(env, d.environment, hasEnv) &&
                selected(crit, d.criticality, hasCrit) &&
                selected(status, d.status, hasStatus);
            node.style('display', visible ? 'element' : 'none');
        });

        state.cy.edges().forEach(function (edge) {
            var srcVisible = edge.source().style('display') !== 'none';
            var tgtVisible = edge.target().style('display') !== 'none';
            edge.style('display', (srcVisible && tgtVisible) ? 'element' : 'none');
        });

        if (state.cy.elements(':visible').length > 0) {
            state.cy.fit(undefined, 40);
        }
    }

    /* ----------------------------------------------------------------------
       Column layout: selected application centered, downstream (dependents)
       on the left, upstream (dependencies) on the right, one column per
       dependency level (L1, L2, ...).
       -------------------------------------------------------------------- */
    function computeColumnPositions(cy, rootId) {
        var outAdj = {};
        var inAdj = {};
        var nodeInfo = {};

        cy.nodes().forEach(function (n) {
            var id = n.id();
            outAdj[id] = [];
            inAdj[id] = [];
            nodeInfo[id] = n.data('name') || id;
        });

        cy.edges().forEach(function (e) {
            outAdj[e.source().id()].push(e.target().id());
            inAdj[e.target().id()].push(e.source().id());
        });

        function bfs(adj, start) {
            var dist = {};
            dist[start] = 0;
            var queue = [start];
            while (queue.length) {
                var u = queue.shift();
                var neighbors = adj[u] || [];
                for (var i = 0; i < neighbors.length; i++) {
                    var v = neighbors[i];
                    if (dist[v] === undefined) {
                        dist[v] = dist[u] + 1;
                        queue.push(v);
                    }
                }
            }
            return dist;
        }

        // Forward distance (root -> depends-on targets) = upstream level;
        // reverse distance (dependents -> root) = downstream level.
        var upDist = bfs(outAdj, rootId);
        var downDist = bfs(inAdj, rootId);

        var meta = {};
        cy.nodes().forEach(function (n) {
            var id = n.id();
            if (id === rootId) {
                meta[id] = { column: 0 };
                return;
            }
            var up = upDist[id];
            var down = downDist[id];
            var side;
            var level;
            if (up !== undefined && down !== undefined) {
                side = down < up ? 'down' : 'up';
                level = Math.min(up, down);
            } else if (up !== undefined) {
                side = 'up';
                level = up;
            } else if (down !== undefined) {
                side = 'down';
                level = down;
            } else {
                side = 'up';
                level = n.data('level') || 1;
            }
            meta[id] = { column: side === 'down' ? -level : level };
        });

        var byColumn = {};
        Object.keys(meta).forEach(function (id) {
            var col = meta[id].column;
            (byColumn[col] = byColumn[col] || []).push(id);
        });

        var H_GAP = 150;
        var V_GAP = 90;
        var positions = {};
        Object.keys(byColumn).forEach(function (col) {
            var ids = byColumn[col];
            ids.sort(function (a, b) {
                return (nodeInfo[a] || '').localeCompare(nodeInfo[b] || '');
            });
            var y0 = -((ids.length - 1) * V_GAP) / 2;
            for (var i = 0; i < ids.length; i++) {
                positions[ids[i]] = { x: parseInt(col, 10) * H_GAP, y: y0 + i * V_GAP };
            }
        });

        return positions;
    }

    function runColumnLayout(cy, rootId) {
        if (!cy || !cy.nodes().length) return;
        cy.layout({
            name: 'preset',
            positions: computeColumnPositions(cy, rootId),
            fit: false,
            animate: true,
            animationDuration: 400
        }).run();
    }

    function runGraphLayout(cy) {
        if (!cy || !cy.nodes().length) return;
        if (state.allMode) {
            runAllLayout(cy);
        } else {
            runColumnLayout(cy, 'app-' + state.rootApplicationId);
        }
    }

    /* "All applications" view: force-directed layout over the full graph. */
    function runAllLayout(cy) {
        cy.layout({
            name: 'cose',
            animate: true,
            animationDuration: 400,
            padding: 40,
            fit: true,
            nodeRepulsion: 120000,
            idealEdgeLength: 90,
            componentSpacing: 90,
            randomize: true
        }).run();
    }

    /* ----------------------------------------------------------------------
       Interactions
       -------------------------------------------------------------------- */
    function bindInteractions() {
        var cy = state.cy;

        cy.on('tap', 'node', function (evt) {
            var node = evt.target;
            var data = node.data();

            // Highlight and show details panel
            cy.elements().removeClass('highlighted-node');
            node.addClass('highlighted-node');
            showNodePanel(data);

            if (node.hasClass('selected')) {
                // second tap on same node -> expand chain
                expandFromNode(node);
            } else {
                cy.elements().removeClass('selected');
                node.addClass('selected');
            }
        });

        cy.on('tap', function (evt) {
            if (evt.target === cy) {
                cy.elements().removeClass('highlighted-node selected');
                hideNodePanel();
            }
        });

        cy.on('tap', 'edge', function (evt) {
            var edge = evt.target;
            var d = edge.data();
            cy.elements().removeClass('highlighted-node');
            showEdgePanel(d);
        });

        cy.on('dbltap', 'node', function (evt) {
            var data = evt.target.data();
            if (data.isRoot) return;
            navigateToApplication(data.applicationId);
        });
    }

    function expandFromNode(node) {
        var applicationId = node.data('applicationId');
        var depth = state.currentDepth;

        $.ajax({
            url: state.graphUrl,
            data: { applicationId: applicationId, depth: depth, direction: state.currentDirection },
            dataType: 'json',
            cache: false
        })
            .done(function (data) {
                if (!data || !data.nodes) return;

                // Merge new nodes/edges into the existing graph
                var existingIds = {};
                state.cy.nodes().forEach(function (n) { existingIds[n.id()] = true; });
                var existingEdgeIds = {};
                state.cy.edges().forEach(function (e) { existingEdgeIds[e.id()] = true; });

                var newNodes = [];
                $.each(data.nodes, function (_, n) {
                    if (!existingIds[n.id]) {
                        newNodes.push({
                            data: {
                                id: n.id,
                                name: n.name,
                                level: n.level,
                                isRoot: n.isRoot,
                                criticality: n.criticality,
                                environment: n.environment,
                                status: n.status,
                                applicationId: n.applicationId,
                                isRetired: n.status === 'Retired'
                            }
                        });
                        existingIds[n.id] = true;
                    }
                });

                var newEdges = [];
                $.each(data.edges, function (_, e) {
                    if (!existingEdgeIds[e.id]) {
                        newEdges.push({
                            data: {
                                id: e.id,
                                source: e.source,
                                target: e.target,
                                type: e.type,
                                criticality: e.criticality,
                                impact: e.impact,
                                frequency: e.frequency,
                                direction: e.direction,
                                level: e.level,
                                dependencyId: e.dependencyId
                            }
                        });
                        existingEdgeIds[e.id] = true;
                    }
                });

                state.cy.add(newNodes.concat(newEdges));
                runGraphLayout(state.cy);

                // Focus on the clicked node
                var focused = state.cy.getElementById(node.id());
                state.cy.animate({
                    center: { eles: focused },
                    zoom: Math.max(state.cy.zoom(), 0.9),
                    duration: 400
                });
            })
            .fail(function () {
                showToast('Failed to expand dependency chain.', 'danger');
            });
    }

    /* ----------------------------------------------------------------------
       Node / edge detail panels
       -------------------------------------------------------------------- */
    function showNodePanel(data) {
        var $panel = $('#node-panel');
        if (!$panel.length) return;

        var criticalityBadge = '<span class="badge ' + critBadgeClass(data.criticality) + '">' + (data.criticality || '-') + '</span>';
        var envBadge = '<span class="badge ' + envBadgeClass(data.environment) + '">' + (data.environment || '-') + '</span>';
        var statusBadge = '<span class="badge ' + statusBadgeClass(data.status) + '">' + (data.status || '-') + '</span>';

        $('#node-panel-title').text(data.name);
        $('#node-panel-body').html(
            '<div class="mb-2">' +
            '  <span class="badge bg-dark">Level L' + data.level + '</span> ' +
            criticalityBadge + ' ' + envBadge + ' ' + statusBadge +
            '</div>' +
            '<div class="mb-1"><strong>ID:</strong> ' + data.applicationId + '</div>' +
            '<button type="button" class="btn btn-sm btn-primary mt-2" onclick="DTCy.navigateFromPanel(' + data.applicationId + ')">' +
            '  View in Graph' +
            '</button> ' +
            '<a href="' + state.detailsUrlTemplate.replace('__ID__', data.applicationId) + '" class="btn btn-sm btn-outline-secondary mt-2">' +
            '  Details' +
            '</a>'
        );
        $panel.removeClass('d-none');
    }

    function showEdgePanel(data) {
        var $panel = $('#node-panel');
        if (!$panel.length) return;

        $('#node-panel-title').text('Dependency');
        $('#node-panel-body').html(
            '<div class="mb-1"><strong>Type:</strong> <span class="badge bg-light text-dark border">' + (data.type || '-') + '</span></div>' +
            '<div class="mb-1"><strong>Criticality:</strong> ' + (data.criticality
                ? '<span class="badge ' + critBadgeClass(data.criticality) + '">' + data.criticality + '</span>'
                : '-') + '</div>' +
            '<div class="mb-1"><strong>Impact:</strong> ' + (data.impact || '-') + '</div>' +
            '<div class="mb-1"><strong>Frequency:</strong> ' + (data.frequency || '-') + '</div>' +
            '<div class="mb-1"><strong>Direction:</strong> ' + (data.direction || '-') + '</div>' +
            '<div class="mb-1"><strong>Level:</strong> L' + data.level + '</div>'
        );
        $panel.removeClass('d-none');
    }

    function hideNodePanel() {
        $('#node-panel').addClass('d-none');
    }

    /* ----------------------------------------------------------------------
       Helpers
       -------------------------------------------------------------------- */
    function updateLevelLegend(nodes) {
        var maxLevel = 0;
        $.each(nodes, function (_, n) {
            if (n.data.level > maxLevel) maxLevel = n.data.level;
        });
        // nothing to render - legend is static CSS
    }

    function critBadgeClass(criticality) {
        switch ((criticality || '').toLowerCase()) {
            case 'critical': return 'bg-danger';
            case 'high': return 'bg-warning text-dark';
            case 'medium': return 'bg-info text-dark';
            case 'low': return 'bg-success';
            default: return 'bg-secondary';
        }
    }

    function envBadgeClass(env) {
        switch ((env || '').toLowerCase()) {
            case 'production': return 'bg-danger';
            case 'staging': return 'bg-warning text-dark';
            case 'development': return 'bg-secondary';
            default: return 'bg-secondary';
        }
    }

    function statusBadgeClass(status) {
        switch ((status || '').toLowerCase()) {
            case 'active': return 'bg-success';
            case 'planned': return 'bg-info text-dark';
            case 'retired': return 'bg-dark';
            default: return 'bg-secondary';
        }
    }

    function showLoading(visible) {
        $('#graph-loading').toggleClass('d-none', !visible);
    }

    function exportGraphPng() {
        if (!state.cy) {
            showToast('Nothing to export yet. Render a graph first.', 'info');
            return;
        }
        try {
            var url = state.cy.png({ full: true, scale: 2, bg: '#ffffff' });
            var name = 'dependency-graph-' + (state.rootApplicationId || 'all') + '.png';
            var link = document.createElement('a');
            link.href = url;
            link.download = name;
            document.body.appendChild(link);
            link.click();
            document.body.removeChild(link);
        } catch (ex) {
            showToast('Export failed.', 'danger');
        }
    }

    function showToast(message, type) {
        if (!window.bootstrap) { alert(message); return; }
        var toastHtml = '<div class="toast align-items-center text-bg-' + type + ' border-0 position-fixed bottom-0 end-0 m-3" role="alert">' +
            '<div class="d-flex"><div class="toast-body">' + message + '</div>' +
            '<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button></div></div>';
        var $toast = $(toastHtml);
        $('body').append($toast);
        var toast = new window.bootstrap.Toast($toast[0], { delay: 3000 });
        toast.show();
        $toast.on('hidden.bs.toast', function () { $(this).remove(); });
    }

    /* ----------------------------------------------------------------------
       Public API
       -------------------------------------------------------------------- */
    cyModule.navigateFromPanel = function (applicationId) {
        // Re-render graph focused on this application
        loadGraph(applicationId, state.currentDirection, state.currentDepth);
        hideNodePanel();
    };

    cyModule.navigateToApplication = navigateToApplication;

    function navigateToApplication(applicationId) {
        loadGraph(applicationId, state.currentDirection, state.currentDepth);
        hideNodePanel();
    }

    cyModule.initGraph = function (options) {
        state.container = $(options.container)[0] || options.container;
        state.canEdit = options.canEdit;
        state.detailsUrlTemplate = options.detailsUrlTemplate;
        state.graphUrl = options.graphUrl || state.graphUrl;

        // Wire toolbar
        $('#graph-controls').on('submit', function (e) {
            e.preventDefault();
            var appId = $('#graph-app').val();
            if (!appId) {
                showToast('Please select an application first.', 'warning');
                return;
            }
            loadGraph(appId === 'all' ? 'all' : parseInt(appId, 10),
                $('#graph-direction').val(), parseInt($('#graph-depth').val(), 10));
        });

        $('#btn-fit').on('click', function () {
            if (state.cy) state.cy.fit(undefined, 40);
        });
        $('#btn-zoom-in').on('click', function () {
            if (state.cy) state.cy.zoom(state.cy.zoom() * 1.25);
        });
        $('#btn-zoom-out').on('click', function () {
            if (state.cy) state.cy.zoom(state.cy.zoom() * 0.8);
        });
        $('#btn-reset').on('click', function () {
            if (state.allMode) {
                loadGraph('all', state.currentDirection, state.currentDepth);
            } else if (state.rootApplicationId) {
                loadGraph(state.rootApplicationId, state.currentDirection, state.currentDepth);
            }
        });
        $('#btn-export-png').on('click', function () {
            exportGraphPng();
        });
        $('#btn-toggle-filters').on('click', function () {
            $('.graph-filters').toggleClass('d-none');
            if (state.cy && !$('.graph-filters').hasClass('d-none')) {
                state.cy.fit(undefined, 40);
            }
        });
        $('#btn-clear-filters').on('click', function () {
            $.each(state.filters, function () {
                $.each(this, function (key) {
                    this[key] = true;
                });
            });
            $('.graph-filters input[type="checkbox"]').prop('checked', true);
            applyFilters();
        });
        $('#btn-expand').on('click', function () {
            var sel = state.cy ? state.cy.$(':selected') : $();
            var node = sel && sel.length ? sel[0] : null;
            if (node && node.isNode && node.isNode()) {
                expandFromNode(node);
            } else {
                var selectedNode = state.cy ? state.cy.elements('.highlighted-node').first() : null;
                if (selectedNode && selectedNode.length) {
                    expandFromNode(selectedNode);
                } else {
                    showToast('Click a node first to expand its chain.', 'info');
                }
            }
        });
        $('#node-panel-close').on('click', function () {
            hideNodePanel();
        });

        // If an application was preselected, load it; otherwise leave the
        // dropdown on "Select application..." and show the empty state until
        // the user picks an application and clicks Render.
        if (options.initialApplicationId) {
            $('#graph-app').val(options.initialApplicationId);
            loadGraph(options.initialApplicationId, options.initialDirection, options.initialDepth);
        } else {
            $('#graph-empty').removeClass('d-none');
        }
    };

    cyModule.initMiniGraph = function (container) {
        state.container = container[0] || container;
        state.canEdit = false;
        state.graphUrl = '/api/Graph';

        var appId = $(state.container).data('application-id');
        if (!appId) return;

        $.ajax({
            url: state.graphUrl,
            data: { applicationId: appId, depth: 2, direction: 'Both' },
            dataType: 'json',
            cache: false
        })
            .done(function (data) {
                if (!data || !data.nodes) return;
                var nodes = $.map(data.nodes, function (n) {
                    return {
                        data: {
                            id: n.id,
                            name: n.name,
                            level: n.level,
                            isRoot: n.isRoot,
                            criticality: n.criticality,
                            environment: n.environment,
                            status: n.status,
                            applicationId: n.applicationId,
                            isRetired: n.status === 'Retired'
                        }
                    };
                });
                var edges = $.map(data.edges, function (e) {
                    return {
                        data: {
                            id: e.id,
                            source: e.source,
                            target: e.target,
                            type: e.type,
                            criticality: e.criticality,
                            impact: e.impact,
                            frequency: e.frequency,
                            direction: e.direction,
                            level: e.level,
                            dependencyId: e.dependencyId
                        }
                    };
                });

                var cy = window.cytoscape({
                    container: state.container,
                    elements: { nodes: nodes, edges: edges },
                    style: buildStyle(),
                    wheelSensitivity: 0,
                    minZoom: 0.1,
                    maxZoom: 2,
                    layout: { name: 'preset', fit: false }
                });
                runColumnLayout(cy, 'app-' + appId);
                cy.fit(undefined, 10);
                cy.userZoomingEnabled(false);
                cy.userPanningEnabled(true);

                cy.on('tap', 'node', function (evt) {
                    var d = evt.target.data();
                    window.location.href = state.detailsUrlTemplate
                        ? state.detailsUrlTemplate.replace('__ID__', d.applicationId)
                        : '/Applications/Details/' + d.applicationId;
                });
            })
            .fail(function () {
                $(state.container).html('<div class="text-muted small p-2">Unable to load mini graph.</div>');
            });
    };

})(window.DTCy);
