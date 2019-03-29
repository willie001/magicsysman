


(function () {
    'use strict';
    angular
        .module("magicsearches", ['ngSanitize'])
        .controller('MainSearchController', MainSearchController)
        .directive("drawSchedule", drawSchedule);

    MainSearchController.$inject = ['$scope', '$http', '$state', 'HandleBusySpinner', 'ShowUserMessages', 'DTOptionsBuilder', 'editableOptions', 'editableThemes', '$cookies', 'moment', 'manageTimeZoneCookie', 'savedJobBookingFactory'];

    function drawSchedule() {

        var myTimeLine = (function (rosterStart, rosterEnd, timeLineWidth, scaleFactor, searchMinutes, rosterDay, weekOneDate, weekTwoDate, jobs, timeIncrement) {

            // Time line calculations    
            let rosterDuration = rosterEnd - rosterStart;
            let windowWidth = window.innerWidth > 1448 ? window.innerWidth : 1447;
            let mainWidth = Math.floor((windowWidth) - scaleFactor);
            let displayGapPerHour = ((((mainWidth) / rosterDuration) * 60) - timeLineWidth) / 4;
            let displayWidthPerHour = Math.floor(displayGapPerHour) + timeLineWidth;
            let displayWidthPerMinute = (displayWidthPerHour / 15);


            // Contracted timeline calculations
            let contractWidth = Math.floor(timeLineWidth / 4);
            let displayWidthPerHourContracted = Math.floor(displayGapPerHour) + contractWidth;
            let displayWidthPerMinuteContracted = (displayWidthPerHourContracted / 15);

            // DOM elements
            let table = document.createElement('table');
            table.classList.add('tableSchedule');

            // tr time line
            let trTimeline = document.createElement('tr');
            trTimeline.classList.add('trTimeline');
            let tdTimeline1 = document.createElement('td');
            tdTimeline1.textContent = rosterDay;
            tdTimeline1.classList.add('rosterDay');
            let tdTimeline2 = document.createElement('td');
            let divTimeLine = document.createElement('div');
            divTimeLine.classList.add('timelineContainer');
            let spanTimeline = document.createElement('span');
            spanTimeline.classList.add('timeLine');

            // tr week one
            let trRowOne = document.createElement('tr');
            trRowOne.classList.add('weekOne');
            let tdRowOne1 = document.createElement('td');
            tdRowOne1.classList.add('dayDate');
            tdRowOne1.textContent = weekOneDate;
            let tdRowOne2 = document.createElement('td');
            let divRowOne = document.createElement('div');
            divRowOne.classList.add('calendarContainer');

            // tr week two
            let trRowTwo = document.createElement('tr');
            trRowTwo.classList.add('weekTwo');
            let tdRowTwo1 = document.createElement('td');
            tdRowTwo1.classList.add('dayDate');
            tdRowTwo1.textContent = weekTwoDate;
            let tdRowTwo2 = document.createElement('td');
            let divRowTwo = document.createElement('div');
            divRowTwo.classList.add('calendarContainer');

            function draw() {

                setupTimeline(false, spanTimeline);
                setupTimeline(true, divRowOne);
                setupTimeline(true, divRowTwo);

                jobs.forEach((job, index, array) => {
                    createJob(job.startTime, job.endTime, job.weekNo == 1 ? divRowOne : divRowTwo, job.jobType, job.suburb, job.jobColor, job.zoneColor, job.job, job.cleaner);
                })

                buildTable();
            }

            draw();

            function offsetLeft(el) {
                var rect = el.getBoundingClientRect(),
                    scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
                return rect.left + scrollLeft
            }

            function calculateWidth(startTime, endTime) {
                if (startTime < 480 && endTime <= 480) {
                    return (endTime - startTime) * displayWidthPerMinuteContracted;
                }

                if (startTime >= 1080 && endTime > 1080) {
                    return (endTime - startTime) * displayWidthPerMinuteContracted;
                }

                if (startTime < 480 && (endTime > 480 && endTime <= 1080)) {
                    return ((480 - startTime) * displayWidthPerMinuteContracted) + ((endTime - 480) * displayWidthPerMinute);
                }

                if (startTime >= 480 && endTime > 1080) {
                    return ((1080 - startTime) * displayWidthPerMinute) + ((endTime - 1080) * displayWidthPerMinuteContracted);
                }

                if (startTime < 480 && endTime > 1080) {
                    return ((480 - startTime) * displayWidthPerMinuteContracted) + (600 * displayWidthPerMinute) + ((endTime - 1080) * displayWidthPerMinuteContracted);
                }
            }

            function calculateTime(pageX, offsetLeft, pxPerMinute, offsetTime, previousTime, timeDiff) {
                let myTime = Math.floor((((pageX - offsetLeft) + timeDiff) / pxPerMinute) + offsetTime);

                return myTime % timeIncrement == 0 ? myTime : previousTime;
            }

            function createJob(startTime, endTime, parentElement, jobType, suburbName, backGround, zoneColor, job, cleaner) {

                let divOuter = document.createElement('div');
                divOuter.className = 'jobDetails';

                let divJob = document.createElement('div');


                let divInner = document.createElement('div');
                let divInnerZone = document.createElement('div');
                divInner.style.paddingLeft = '3px';

                if (jobType == 'F' || jobType == 'W') {
                    divInner.innerHTML = '<i class="fa fa-repeat"></i> <strong>' + jobType + '</strong> ' + suburbName;
                } else {
                    divInner.innerHTML = '<strong>' + jobType + '</strong> ' + suburbName;
                }

                let width = (endTime - startTime) * displayWidthPerMinute;
                let position = displayWidthPerMinute * (startTime - rosterStart) + 1;

                if (startTime < 480 || startTime > 1080) {
                    position = displayWidthPerMinuteContracted * ((startTime - rosterStart) + 1);
                } else {
                    position = (displayWidthPerMinuteContracted * 120) + (displayWidthPerMinute * (startTime - 480)) + 1;
                }

                if (startTime >= 1080) {
                    position = (displayWidthPerMinuteContracted * 120) + (displayWidthPerMinute * 600) + (displayWidthPerMinuteContracted * (startTime - 1080)) + 1;
                }

                if (startTime < 480 && endTime <= 480) {
                    width = (endTime - startTime) * displayWidthPerMinuteContracted;
                }

                if (startTime >= 1080 && endTime > 1080) {
                    width = (endTime - startTime) * displayWidthPerMinuteContracted;
                }

                if (startTime < 480 && (endTime > 480 && endTime <= 1080)) {
                    width = ((480 - startTime) * displayWidthPerMinuteContracted) + ((endTime - 480) * displayWidthPerMinute);
                }

                if (startTime >= 480 && endTime > 1080) {
                    width = ((1080 - startTime) * displayWidthPerMinute) + ((endTime - 1080) * displayWidthPerMinuteContracted);
                }

                if (startTime < 480 && endTime > 1080) {
                    width = ((480 - startTime) * displayWidthPerMinuteContracted) + (600 * displayWidthPerMinute) + ((endTime - 1080) * displayWidthPerMinuteContracted);
                }

                divInnerZone.style.backgroundColor = zoneColor;
                divInnerZone.style.width = '13px';
                divInnerZone.style.height = '13px';
                divInnerZone.style.borderRadius = '50%';
                divInnerZone.style.marginRight = '2px';
                divInnerZone.style.border = '1px solid black';
                divInnerZone.style.marginTop = '1px';
                divInnerZone.style.float = 'right';
                divInner.style.overflow = 'hidden';
                divInner.style.textOverflow = 'ellipsis';

                divOuter.style.width = width + 'px';
                divOuter.style.backgroundColor = backGround;
                divOuter.style.position = 'absolute';
                divOuter.style.left = position + 'px';

                if (jobType == 'A') {
                    divInnerZone.style.backgroundColor = backGround;
                    divInnerZone.style.border = '';
                }

                if ((endTime - startTime) >= searchMinutes && jobType == 'A') {

                    divJob.style.width = (searchMinutes * displayWidthPerMinute) + 'px';
                    divJob.style.height = '15px';
                    divJob.style.backgroundColor = backGround;
                    divJob.style.display = 'inline-block';
                    divJob.style.position = 'absolute';
                    divJob.style.textAlign = 'center';
                    divJob.style.overflow = 'hidden';
                    divJob.style.textOverflow = 'ellipsis';
                    divOuter.appendChild(divJob);

                    let sTime = startTime;
                    let eTime = startTime + searchMinutes;

                    divJob.innerText = convertMinsToHrsMinsFull(sTime) + ' - ' + convertMinsToHrsMinsFull(eTime);
                    divJob.style.width = calculateWidth(startTime, startTime + searchMinutes) + 'px';

                    let isDown = false;

                    addEvent(divJob, 'mousedown', function (e) {
                        isDown = true;
                        divJob.style.cursor = 'w-resize';
                    });

                    addEvent(document, 'mouseup', function () {
                        isDown = false;
                        divJob.style.cursor = 'default';
                    });

                    addEvent(document, "mousemove", function (e) {
                        e.preventDefault();
                        if (isDown) {

                            //Office hours
                            if (startTime >= 480 && endTime <= 1080) {
                                if (e.pageX >= offsetLeft(divOuter) && e.pageX <= (offsetLeft(divOuter) + divOuter.offsetWidth - (searchMinutes * displayWidthPerMinute))) {
                                    sTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinute, startTime, sTime, 0);
                                    eTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinute, startTime + searchMinutes, eTime, 0);                                   

                                    divJob.style.left = (e.pageX - offsetLeft(divOuter)) + 'px';
                                }
                            }

                            //After hours
                            if ((startTime < 480 && endTime <= 480) || (startTime >= 1080 && endTime > 1080)) {
                                if (e.pageX >= offsetLeft(divOuter) && e.pageX <= (offsetLeft(divOuter) + divOuter.offsetWidth - (searchMinutes * displayWidthPerMinuteContracted))) {
                                    sTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinuteContracted, startTime, sTime, 0);
                                    eTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinuteContracted, startTime + searchMinutes, eTime, 0);
                                    
                                    divJob.style.left = (e.pageX - offsetLeft(divOuter)) + 'px';
                                }
                            }

                            //Early hours + Office hours
                            if (startTime < 480 && (endTime > 480 && endTime <= 1080)) {
                                if (e.pageX >= offsetLeft(divOuter) && e.pageX <= (offsetLeft(divOuter) + divOuter.offsetWidth - (searchMinutes * displayWidthPerMinute))) {
                                    if (sTime < 480) {
                                        sTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinuteContracted, startTime, sTime, 0);
                                        eTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinuteContracted, startTime + searchMinutes, eTime, 0);                                        
                                    }

                                    let smallWidth = (480 - startTime) * displayWidthPerMinuteContracted;
                                    let diff = (displayWidthPerMinute - displayWidthPerMinuteContracted) * smallWidth;

                                    if (sTime >= 480) {
                                        sTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinute, startTime, sTime, diff);
                                        eTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinute, startTime + searchMinutes, eTime, diff);                                       
                                    }

                                    divJob.style.left = (e.pageX - offsetLeft(divOuter)) + 'px';
                                }
                            }

                            //Office hours + Late hours
                            if ((startTime >= 480 && startTime < 1080) && endTime > 1080) {

                                let offsetRight = searchMinutes * displayWidthPerMinuteContracted;
                                let minutesContracted = endTime - 1080;
                                let minutesNormal = 0;

                                if (searchMinutes > minutesContracted) {
                                    minutesNormal = searchMinutes - minutesContracted;
                                    offsetRight = (minutesNormal * displayWidthPerMinute) + (minutesContracted * displayWidthPerMinuteContracted);
                                }

                                if (e.pageX >= offsetLeft(divOuter) && e.pageX <= (offsetLeft(divOuter) + divOuter.offsetWidth - offsetRight)) {
                                    if (sTime < 1080) {
                                        sTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinute, startTime, sTime, 0);
                                        eTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinute, startTime + searchMinutes, eTime, 0);                                        
                                    }

                                    let bigWidth1 = (1080 - startTime) * displayWidthPerMinute;  //1080
                                    let smallWidth1 = (1080 - startTime) * displayWidthPerMinuteContracted //600
                                    let bigDiff1 = smallWidth1 - bigWidth1

                                    if (sTime >= 1080) {                                       
                                        sTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinuteContracted, startTime, sTime, bigDiff1);
                                        eTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinuteContracted, startTime + searchMinutes, eTime, bigDiff1);                                        
                                    }

                                    divJob.style.left = (e.pageX - offsetLeft(divOuter)) + 'px';
                                }
                            }

                            //Early hours + Normal hours + Late hours
                            if (startTime < 480 && endTime > 1080) {

                                if (e.pageX >= offsetLeft(divOuter) && e.pageX <= (offsetLeft(divOuter) + divOuter.offsetWidth - (searchMinutes * displayWidthPerMinuteContracted))) {

                                    if (sTime < 480) {
                                        sTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinuteContracted, startTime, sTime, 0);
                                        eTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinuteContracted, startTime + searchMinutes, eTime, 0);
                                    }

                                    let smallWidth2 = (480 - startTime) * displayWidthPerMinuteContracted;
                                    let diff2 = (displayWidthPerMinute - displayWidthPerMinuteContracted) * smallWidth2;

                                    if (sTime >= 480) {
                                        sTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinute, startTime, sTime, diff2);
                                        eTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinute, startTime + searchMinutes, eTime, diff2);                                        

                                        let bigWidth3 = (1080 - startTime) * displayWidthPerMinute;  
                                        let smallWidth3 = (1080 - startTime) * displayWidthPerMinuteContracted 
                                        let bigDiff3 = smallWidth3 - bigWidth3
                                        let mainDiff = diff2 + bigDiff3;

                                        if (sTime >= 1080) {
                                            sTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinuteContracted, startTime, sTime, mainDiff);
                                            eTime = calculateTime(e.pageX, offsetLeft(divOuter), displayWidthPerMinuteContracted, startTime + searchMinutes, eTime, mainDiff);
                                        }

                                    }

                                    divJob.style.left = (e.pageX - offsetLeft(divOuter)) + 'px';
                                }
                            }

                            divJob.style.width = calculateWidth(sTime, eTime) + 'px';
                            divJob.innerText = convertMinsToHrsMinsFull(Math.floor(sTime)) + ' - ' + convertMinsToHrsMinsFull(Math.floor(eTime));

                            job.StartTime = Math.floor(sTime);
                            job.EndTime = Math.floor(eTime);
                            job.StartTimeForControl = '2000-01-01T' + convertMinsToHrsMinsFull(Math.floor(sTime)) + ':00'
                            job.EndTimeForControl = '2000-01-01T' + convertMinsToHrsMinsFull(Math.floor(eTime)) + ':00'
                            job.StartTimeOfDay = convertMinsToHrsMinsFull(Math.floor(sTime))
                            job.EndTimeOfDay = convertMinsToHrsMinsFull(Math.floor(eTime))
                        }

                    });

                    addEvent(divJob, 'dblclick', function () {

                        if ((job.JobDate != null) && (job.JobType == 2 || job.JobType == 3)) {
                            job.JobDescription = job.WeekDay + ' on ' + job.JobDate + ' (' + job.StartTimeOfDay + ' - ' + job.EndTimeOfDay + ')';
                        }
                        else {
                            job.JobDescription = job.WeekDay + ' (' + job.StartTimeOfDay + ' - ' + job.EndTimeOfDay + ')';
                        }

                        var scope = angular.element(document.getElementById("mainWrap")).scope();
                        scope.$apply(function () {
                            scope.checkCustomerState(cleaner, job);
                        });

                    });

                }

                if ((endTime - startTime) >= searchMinutes && jobType == 'A') {
                    divOuter.style.backgroundColor = '#53bc78';
                    divInnerZone.style.backgroundColor = '#53bc78';
                }

                if ((endTime - startTime) < searchMinutes && jobType == 'A') {
                    divOuter.style.backgroundColor = '#f46969';
                    divInnerZone.style.backgroundColor = '#f46969';
                }

                divOuter.appendChild(divInnerZone);
                divOuter.appendChild(divInner);
                parentElement.appendChild(divOuter);
            }

            function setupTimeline(noHours, boundControl) {
                let output = '';
                let i = rosterStart;

                while (i >= rosterStart && i <= rosterEnd) {
                    let hour = '';
                    let gap = Math.floor(displayGapPerHour);
                    let width = timeLineWidth;
                    let color = i % 60 == 0 ? '#383838' : '#999999';

                    if (!noHours) hour = i % 60 == 0 ? convertMinsToHrsMins(i) : ''; //This is for the background markers in the row itself

                    if (i < 480 || i >= 1080) { //This is to contract the "over time" hours to save space                
                        width = contractWidth;
                        hour = '';
                    }

                    output = output + '</span><span style="width: ' + width + 'px; display: inline-block; color: ' + color + ';">|' + hour + '</span><span style="width: ' + gap + 'px; display: inline-block">';
                    i = i + 15;

                }
                boundControl.innerHTML = output;

            }

            function convertMinsToHrsMins(mins) {
                let h = Math.floor(mins / 60);
                h = h < 10 ? '0' + h : h;
                return `${h}`;
            }

            function convertMinsToHrsMinsFull(mins) {
                let h = Math.floor(mins / 60);
                let m = mins % 60;
                h = h < 10 ? '0' + h : h;
                m = m < 10 ? '0' + m : m;
                return `${h}:${m}`;
            }

            function buildTable() {
                // Connect all table nodes
                divTimeLine.appendChild(spanTimeline);
                tdTimeline2.appendChild(divTimeLine);
                trTimeline.appendChild(tdTimeline1);
                trTimeline.appendChild(tdTimeline2);

                tdRowOne2.appendChild(divRowOne);
                trRowOne.appendChild(tdRowOne1);
                trRowOne.appendChild(tdRowOne2);

                tdRowTwo2.appendChild(divRowTwo);
                trRowTwo.appendChild(tdRowTwo1);
                trRowTwo.appendChild(tdRowTwo2);

                table.appendChild(trTimeline);
                table.appendChild(trRowOne);
                table.appendChild(trRowTwo);
            }

            return table;

        });

        function addEvent(object, type, callback) {
            if (object == null || typeof (object) == 'undefined') return;
            if (object.addEventListener) {
                object.addEventListener(type, callback, false);
            } else if (object.attachEvent) {
                object.attachEvent("on" + type, callback);
            } else {
                object["on" + type] = callback;
            }
        }

        const JOBCOLOR = {
            AVAILABLE: '#696969',
            NOTAVAILABLE: '#af5903' //#F47F7F'
        }

        const ZONECOLOR = {
            PRIMARY: '#35b5ff',
            SECONDARY: '#32ff7d',
            APPROVED: '#febc02'
        }

        function getJobColor(jobClass) {
            let color = ZONECOLOR.PRIMARY

            if (jobClass == 'colourMatch_Green') color = ZONECOLOR.SECONDARY;
            if (jobClass == 'colourMatch_Orange') color = ZONECOLOR.APPROVED;
            //console.log(color);
            return color
        }

        function init(cleaner, searchCriteria) {

            //console.log(cleaner);
            //console.log(searchCriteria)
            let rosterDay = cleaner.DisplaySelectedRosterDay;
            let dateWeek1 = cleaner.DisplayServiceDate;
            let dateWeek2 = cleaner.DisplayServiceDateNextWeek;

            let serviceLengthDate = new Date(searchCriteria.ServiceLengthForControl);
            let serviceHours = serviceLengthDate.getHours();
            let serviceMinutes = serviceLengthDate.getMinutes();
            let minutes = Math.floor(((serviceHours * 60) + serviceMinutes) / cleaner.TeamSize);

            //console.log("Service Length: " + minutes);

            let jobs = [];

            if (cleaner.ScheduledJobs) {
                cleaner.ScheduledJobs.forEach(job => {
                    let job1 = {};

                    if (job.JobStatus == 'AVAILABLE') {
                        job1 = {
                            startTime: job.StartTime,
                            endTime: job.EndTime,
                            weekNo: 1,
                            jobType: 'A',
                            suburb: '',
                            jobColor: JOBCOLOR.AVAILABLE,
                            zoneColor: getJobColor(job.JobColourCode),
                            job: job,
                            cleaner: cleaner
                        }
                    } else {
                        job1 = {
                            startTime: job.StartTime,
                            endTime: job.EndTime,
                            weekNo: 1,
                            jobType: job.JobTypeName.charAt(0),
                            suburb: job.JobSuburb,
                            jobColor: JOBCOLOR.NOTAVAILABLE,
                            zoneColor: getJobColor(job.JobColourCode)
                        }
                    }

                    jobs.push(job1);
                });
            }

            if (cleaner.ScheduledJobsNextWeek) {
                cleaner.ScheduledJobsNextWeek.forEach(job => {
                    let job2 = {};

                    if (job.JobStatus == 'AVAILABLE') {
                        job2 = {
                            startTime: job.StartTime,
                            endTime: job.EndTime,
                            weekNo: 2,
                            jobType: 'A',
                            suburb: '',
                            jobColor: JOBCOLOR.AVAILABLE,
                            zoneColor: getJobColor(job.JobColourCode),
                            job: job,
                            cleaner: cleaner
                        }
                    } else {
                        job2 = {
                            startTime: job.StartTime,
                            endTime: job.EndTime,
                            weekNo: 2,
                            jobType: job.JobTypeName.charAt(0),
                            suburb: job.JobSuburb,
                            jobColor: JOBCOLOR.NOTAVAILABLE,
                            zoneColor: getJobColor(job.JobColourCode)
                        }
                    }

                    jobs.push(job2);
                });
            }

            return myTimeLine(360, 1200, 15, 1000, minutes, rosterDay, dateWeek1, dateWeek2, jobs, 5);
        }

        return {
            scope: {
                cleaner: '=',
                searchCriteria: '='
            },

            link: function (scope, element) {
                element.html(init(scope.cleaner, scope.searchCriteria));
            }
        };
    }

    /***********************/
    /***   MAIN SEARCH   ***/
    /***********************/
    function MainSearchController($scope, $http, $state, HandleBusySpinner, ShowUserMessages, DTOptionsBuilder, editableOptions, editableThemes, $cookies, moment, manageTimeZoneCookie, savedJobBookingFactory) {
        var vm = this;
        var panelName = "panelMainResults";

        $scope.changeServiceType = function () {
            vm.Search.WeeklyJob = (vm.Search.ServiceType == "W") ? true : false;
            vm.Search.FortnightlyJob = (vm.Search.ServiceType == "F") ? true : false;
            vm.Search.OneOffJob = (vm.Search.ServiceType == "O") ? true : false;
            vm.Search.VacateClean = (vm.Search.ServiceType == "V") ? true : false;
        }

        $scope.changeZoneFilter = function () {
            if (vm.Search.FilterZonesNone == true) {
                // ensure only vacate or once off selected for ALL zone filter - need specific date
                if (vm.Search.ServiceType == "W" || vm.Search.ServiceType == "F") {
                    vm.Search.ServiceType = "O";
                    vm.Search.WeeklyJob = false;
                    vm.Search.FortnightlyJon = false;
                    vm.Search.OneOffJob = true;
                }
            }
        };

        $scope.changeServiceDay = function () {
            if (vm.Search.ServiceDayValue == 1)
                vm.Search.ServiceDay = "Monday";

            if (vm.Search.ServiceDayValue == 2)
                vm.Search.ServiceDay = "Tuesday";

            if (vm.Search.ServiceDayValue == 3)
                vm.Search.ServiceDay = "Wednesday";

            if (vm.Search.ServiceDayValue == 4)
                vm.Search.ServiceDay = "Thursday";

            if (vm.Search.ServiceDayValue == 5)
                vm.Search.ServiceDay = "Friday";

            if (vm.Search.ServiceDayValue == 6)
                vm.Search.ServiceDay = "Saturday";

            if (vm.Search.ServiceDayValue == 7)
                vm.Search.ServiceDay = "Sunday";

            vm.Search.ServiceDayValue = vm.Search.ServiceDayValue.toString();	// does not like it if the value is integer
        };

        $scope.clearForm = function () {
            var cookieName = "SearchCriteria_cleanerMatch";
            savedJobBookingFactory.set(null, null);
            if ($cookies.get(cookieName)) { // browser supports cookie
                $cookies.remove(cookieName);
                activate();
            }
        };

        $scope.getLocation = function (val) {
            return $http.get('/search/getsearchsuburbs', {
                params: {
                    address: val,
                    sensor: false
                }
            }).then(function (res) {
                var addresses = [];
                angular.forEach(res.data.item, function (value) {
                    /*jshint -W106*/
                    addresses.push(value);
                });
                vm.SuburbList = addresses;
                //console.log("<SEARCH Suburb List> - " + angular.toJson(vm.SuburbList));

            });
        };

        $scope.checkCustomerState = function (selectedCleaner, selectedJob) {
            //console.log("<JOB PICKED -  cleaner> - " + angular.toJson(selectedCleaner));
            //console.log("<JOB PICKED -  job> - " + angular.toJson(selectedJob));

            //console.log(selectedJob.StartTime);
            //console.log(selectedJob.EndTime);
            selectedJob.TeamSize = selectedCleaner.TeamSize;
            savedJobBookingFactory.set(selectedCleaner, selectedJob);

            if (vm.Search.RepeatCustomer == "N") {
                $state.go("app.client_details");
            }
            else {
                $state.go("app.clients");
            }
        };

        activate();

        function activate() {
            vm.Search = {};
            vm.SeachResults = {};
            vm.Search.FilterZone = {};
            vm.hasSearched = false;
            vm.SuburbList = [];

            $scope.getLocation();

            $scope.userMessages = [];
            $scope.userMessageType = [];

            HandleBusySpinner.start($scope, panelName);
            manageTimeZoneCookie.set($cookies, moment, location);

            $scope.dtOptions = DTOptionsBuilder.newOptions().withOption('order', [1, 'desc']);

            editableOptions.theme = 'bs3';

            editableThemes.bs3.inputClass = 'input-sm';
            editableThemes.bs3.buttonsClass = 'btn-sm';
            editableThemes.bs3.submitTpl = '<button type="submit" class="btn btn-success"><span class="fa fa-check"></span></button>';
            editableThemes.bs3.cancelTpl = '<button type="button" class="btn btn-default" ng-click="$form.$cancel()">' +
                '<span class="fa fa-times text-muted"></span>' +
                '</button>';

            vm.Search.Ironing = false;
            var _dfltGap = 2;
            vm.Search.ServiceLengthMins = _dfltGap * 60;
            vm.Search.ServiceLengthForControl = new Date(2001, 1, 1, _dfltGap, 0);
            vm.Search.ServiceDate = new Date();
            vm.Search.ServiceType = "W"
            vm.Search.ServiceDayValue = "1";
            vm.Search.FilterZonesPrimary = true;
            vm.Search.FilterZonesSecondary = true;
            vm.Search.FilterZonesApproved = false;
            vm.Search.FilterZonesOther = false;
            vm.Search.RepeatCustomer = "Y";

            vm.isMeridian = true;
            vm.hrStep = 1;
            vm.minStep = 5;

            $scope.changeServiceType();
            $scope.changeServiceDay();

            vm.date = [];
            vm.date.clear = function () {
                vm.Search.ServiceDate = null;
            };

            // Disable weekend selection
            vm.date.disabled = function (date, mode) {
                //return ( mode === 'day' && ( date.getDay() === 0 || date.getDay() === 6 ) );
                return (mode === 'day' && (date.getDay() === 0));
            };

            vm.date.toggleDateRange = function () {
                vm.date.minDate = new Date();
                vm.date.maxDate = vm.date.minDate.setMonth(vm.date.minDate.getMonth() + 6);
            };
            vm.date.toggleDateRange();

            vm.date.open = function ($event) {
                $event.preventDefault();
                $event.stopPropagation();

                vm.date.opened = true;
            };

            vm.date.dateOptions = {
                formatYear: 'yyyy',
                startingDay: 1
            };

            vm.date.initDate = new Date();
            vm.date.formats = ['dd-MM-yyyy', 'yyyy/MM/dd', 'dd.MM.yyyy', 'shortDate'];
            vm.date.format = vm.date.formats[0];

            //var criteria = {};
            //$http.get('/search/GetSearchCriteria/')
            //             .success(function (data) {
            //                 criteria = data.item;
            //		if (criteria.HasCriteria == true)
            //		{
            //			vm.Search = criteria;
            //			$scope.changeServiceType();
            //			$scope.changeServiceDay();
            //			vm.Search.FilterRating = vm.Search.FilterRating.toString();		// does not like it if the value is integer
            //			vm.Search.ServiceDate = new Date(vm.Search.ServiceDate);		// uib-datepicker is blank when repopulated even if date is set

            //			$scope.searchMatches();
            //		}
            //		//console.log("<SEARCH Criteria> - " + angular.toJson(data.item));

            //             }).error(function(err) {
            //             }).finally(function() {
            //	});

            $scope.searchCriteria = false; // expand search panel on first load
        }

        $scope.searchMatches = function () {
            ShowUserMessages.clear($scope);
            vm.hasSearched = true;
            HandleBusySpinner.start($scope, panelName);

            //console.log(vm.Search);
            //console.log("<CLIENT Search> - " + angular.toJson(vm.Search));
            //console.log("1. Search Mins: " + vm.Search.ServiceLengthMins)
            //console.log("1. Search Control: " + vm.Search.ServiceLengthForControl)

            $http.post('/search/matchcleaners', vm.Search).success(function (response) {
                //console.log("<MAIN Search Results> - " + angular.toJson(response));
                if (!response.IsValid && response.IsValid !== undefined) {
                    HandleBusySpinner.stop($scope, panelName);
                    ShowUserMessages.show($scope, response, "Error performing search.");
                    vm.hasSearched = false;
                }
                else {
                    $scope.searchCriteria = false;
                    vm.SearchResults = response.SearchResults;
                    HandleBusySpinner.stop($scope, panelName);
                    //console.log(vm.SearchResults);
                    //console.log("2. Search Mins: " + vm.Search.ServiceLengthMins)
                    //console.log("2. Search Control: " + vm.Search.ServiceLengthForControl)
                }

            }).error(function (error) {
                //console.log("<MAIN Search Errors> - " + angular.toJson(error));
                HandleBusySpinner.stop($scope, panelName);
                ShowUserMessages.show($scope, error, "Error performing search.");
                vm.hasSearched = false;
            });


        };

        $scope.validateData = function (data, colName) {
            //console.log("<MAIN Search validate> - " + angular.toJson(data));
            if (data.length == 0) {
                return colName + ' is mandatory';
            }
        };
    }

})();